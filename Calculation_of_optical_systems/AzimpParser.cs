using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Calculation_of_optical_systems
{
    public class AzimpParser
    {
        private const string BaseUrl = "https://azimp.ru";
        private const string CatalogUrl = "https://azimp.ru/catalogue/cameras-lenses/";
        private const string JsonFile = "azimp_lenses.json";

        private readonly HttpClient _client = new HttpClient();

        // Основной метод: парсим все страницы
        public async Task<List<LensParser.Lens>> ParseAllPagesAsync()
        {
            var allLenses = new List<LensParser.Lens>();
            int page = 1;

            while (true)
            {
                Console.WriteLine($"Парсим страницу {page}...");
                var pageUrl = CatalogUrl + $"?PAGEN_1={page}";

                string html;
                try
                {
                    html = await _client.GetStringAsync(pageUrl);
                }
                catch
                {
                    Console.WriteLine("Ошибка при загрузке страницы.");
                    break;
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Ищем все изображения товара
                var nodes = doc.DocumentNode.SelectNodes("//img[contains(@class,'img-responsive') and contains(@class,'lazyloaded')]");
                if (nodes == null || nodes.Count == 0)
                {
                    Console.WriteLine("Больше страниц нет, парсинг завершён.");
                    break;
                }

                foreach (var node in nodes)
                {
                    var lens = new LensParser.Lens();

                    // Название
                    lens.Model = node.GetAttributeValue("alt", node.GetAttributeValue("title", "")).Trim();

                    // Ссылка на товар (если <img> обернут в <a>)
                    var parent = node.ParentNode;
                    lens.ProductUrl = (parent != null && parent.Name == "a")
                        ? BaseUrl + parent.GetAttributeValue("href", "")
                        : "";

                    // Картинка (data-src или src)
                    lens.ImageUrl = node.GetAttributeValue("data-src", node.GetAttributeValue("src", ""));

                    if (!string.IsNullOrWhiteSpace(lens.Model))
                        allLenses.Add(lens);
                }

                page++;
                await Task.Delay(150); // пауза между страницами
            }

            Console.WriteLine($"Всего товаров найдено: {allLenses.Count}");
            return RemoveDuplicates(allLenses);
        }

        // Метод для сохранения в JSON
        public async Task SaveToJsonAsync(List<LensParser.Lens> lenses)
        {
            var parser = new LensParser();
            await parser.SaveToJsonAsync(lenses, JsonFile);
        }

        // Метод для загрузки из JSON
        public async Task<List<LensParser.Lens>> LoadFromJsonAsync()
        {
            var parser = new LensParser();
            return await parser.LoadFromJsonAsync(JsonFile);
        }

        // Убираем дубликаты по названию
        private List<LensParser.Lens> RemoveDuplicates(List<LensParser.Lens> lenses)
        {
            return lenses
                .Where(l => !string.IsNullOrWhiteSpace(l.Model))
                .GroupBy(l => l.Model.Trim().ToLower())
                .Select(g => g.First())
                .ToList();
        }
    }
}