using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Calculation_of_optical_systems
{
    public class LensParser
    {
        // Модель объектива, в которую мы сохраняем данные из API
        public class Lens
        {
            public string Model { get; set; }        // Название модели
            public string SensorFormat { get; set; } // Формат матрицы
            public string FocalLength { get; set; }  // Фокусное расстояние
            public string ProductUrl { get; set; }   // Ссылка на товар
            public string ImageUrl { get; set; }     // Ссылка на изображение
        }

        // Основной метод парсинга через WooCommerce Store API
        public async Task<List<Lens>> ParseLensesAsync()
        {
            // Создаём HTTP-клиент для запросов к API
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            // Список для хранения полученных объективов
            var parsedLenses = new List<Lens>();

            int page = 1;          // Текущая страница API
            int perPage = 100;     // Сколько товаров запрашивать за один раз

            // Листаем страницы API, пока товары не закончатся
            while (true)
            {
                string url = $"https://cctvlens.ru/wp-json/wc/store/products?page={page}&per_page={perPage}";
                Console.WriteLine($"API страница {page}");

                HttpResponseMessage response;

                try
                {
                    // Отправляем GET-запрос
                    response = await client.GetAsync(url);
                }
                catch
                {
                    // Если произошла ошибка соединения — выходим
                    break;
                }

                // Если сервер вернул ошибку — прекращаем
                if (!response.IsSuccessStatusCode)
                    break;

                // Читаем JSON-ответ
                var json = await response.Content.ReadAsStringAsync();

                // Десериализуем список товаров
                var products = JsonSerializer.Deserialize<List<JsonElement>>(json);

                // Если товаров больше нет — выходим из цикла
                if (products == null || products.Count == 0)
                    break;

                // Обрабатываем каждый товар
                foreach (var product in products)
                {
                    var lens = new Lens();

                    // Название товара
                    if (product.TryGetProperty("name", out var nameProp))
                        lens.Model = nameProp.GetString();

                    // Ссылка на страницу товара
                    if (product.TryGetProperty("permalink", out var linkProp))
                        lens.ProductUrl = linkProp.GetString();

                    // Получение первой картинки товара
                    if (product.TryGetProperty("images", out var imagesProp) &&
                        imagesProp.ValueKind == JsonValueKind.Array &&
                        imagesProp.GetArrayLength() > 0)
                    {
                        var firstImage = imagesProp[0];

                        if (firstImage.TryGetProperty("src", out var srcProp))
                            lens.ImageUrl = srcProp.GetString();
                    }

                    // Обработка атрибутов товара (формат матрицы, фокусное расстояние)
                    if (product.TryGetProperty("attributes", out var attributesProp) &&
                        attributesProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var attr in attributesProp.EnumerateArray())
                        {
                            // Название атрибута
                            if (!attr.TryGetProperty("name", out var attrNameProp))
                                continue;

                            string attrName = attrNameProp.GetString();

                            // Значение хранится в массиве terms
                            if (!attr.TryGetProperty("terms", out var termsProp) ||
                                termsProp.ValueKind != JsonValueKind.Array ||
                                termsProp.GetArrayLength() == 0)
                                continue;

                            var term = termsProp[0];

                            if (!term.TryGetProperty("name", out var valueProp))
                                continue;

                            string value = valueProp.GetString();

                            // Проверяем по части названия атрибута
                            if (!string.IsNullOrEmpty(attrName))
                            {
                                if (attrName.Contains("Фокус"))
                                    lens.FocalLength = value;

                                if (attrName.Contains("Формат"))
                                    lens.SensorFormat = value;
                            }
                        }
                    }

                    // Добавляем товар, если у него есть модель
                    if (!string.IsNullOrWhiteSpace(lens.Model))
                        parsedLenses.Add(lens);
                }

                page++;                // Переходим на следующую страницу
                await Task.Delay(200); // Небольшая пауза, чтобы не спамить сервер
            }

            Console.WriteLine($"Всего получено через API: {parsedLenses.Count}");

            // Сохраняем результат в JSON-файл
            return await SaveToJsonAsync(parsedLenses);
        }

        // Метод сохранения и объединения данных с уже существующим файлом
        private async Task<List<Lens>> SaveToJsonAsync(List<Lens> newLenses)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true // Красивое форматирование JSON
            };

            List<Lens> existing = new();

            // Если файл уже существует — читаем старые данные
            if (File.Exists("objectivy.json"))
            {
                try
                {
                    string oldJson = await File.ReadAllTextAsync("objectivy.json");

                    existing = JsonSerializer.Deserialize<List<Lens>>(oldJson)
                               ?? new List<Lens>();
                }
                catch
                {
                    // Если файл повреждён — начинаем заново
                    existing = new List<Lens>();
                }
            }

            // Добавляем новые данные к старым
            existing.AddRange(newLenses);

            // Удаляем дубликаты (по модели + фокусному расстоянию)
            var merged = existing
                .Where(l => !string.IsNullOrWhiteSpace(l.Model))
                .GroupBy(l => $"{l.Model?.Trim().ToLower()}_{l.FocalLength?.Trim().ToLower()}")
                .Select(g => g.First())
                .ToList();

            // Сериализуем итоговый список
            string json = JsonSerializer.Serialize(merged, jsonOptions);

            // Перезаписываем файл
            await File.WriteAllTextAsync("objectivy.json", json);

            return merged;
        }
    }
}