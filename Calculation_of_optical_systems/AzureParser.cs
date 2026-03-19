using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Calculation_of_optical_systems
{
    class AzureParser
    {
        private const string BaseUrl = "https://cameralab.ru";
        private const string CatalogUrl = "https://cameralab.ru/catalog/obektivy-azure/s-postoyannym-fokusom-2mp/";

        public async Task<List<LensParser.Lens>> ParseAsync()
        {
            var lenses = new List<LensParser.Lens>();
            using var client = new HttpClient();

            // Обязательно для AJAX-запроса
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

            int page = 1;
            bool hasMore = true;

            while (hasMore)
            {
                var url = $"{CatalogUrl}?PAGEN_1={page}&AJAX_REQUEST=Y&ajax_get=Y&bitrix_include_areas=N";
                var html = await client.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // XPath для карточек товара
                var nodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'product')]");

                if (nodes == null || nodes.Count == 0)
                {
                    hasMore = false; // больше страниц нет
                    break;
                }

                foreach (var node in nodes)
                {
                    try
                    {
                        var lens = ParseLens(node);
                        if (!string.IsNullOrWhiteSpace(lens.Model))
                            lenses.Add(lens);
                    }
                    catch
                    {
                        // пропускаем кривые элементы
                    }
                }

                Console.WriteLine($"Страница {page}: найдено {nodes.Count} товаров");
                page++;
            }

            Console.WriteLine($"Всего найдено: {lenses.Count} товаров");
            return lenses;
        }

        private LensParser.Lens ParseLens(HtmlNode node)
        {
            var lens = new LensParser.Lens();

            // Название
            var nameNode = node.SelectSingleNode(".//h2[contains(@class,'woocommerce-loop-product__title')]");
            if (nameNode != null)
                lens.Model = HtmlEntity.DeEntitize(nameNode.InnerText.Trim());

            // Ссылка
            var linkNode = node.SelectSingleNode(".//a[contains(@href,'product')]");
            if (linkNode != null)
            {
                var href = linkNode.GetAttributeValue("href", "");
                lens.ProductUrl = href.StartsWith("http") ? href : BaseUrl + href;
            }

            // Картинка (lazy loading поддерживается)
            var imgNode = node.SelectSingleNode(".//img");
            if (imgNode != null)
            {
                var src = imgNode.GetAttributeValue("src", "");
                if (string.IsNullOrEmpty(src))
                    src = imgNode.GetAttributeValue("data-src", "");
                lens.ImageUrl = src;
            }

            // Текст для фокуса и матрицы
            var text = node.InnerText.ToLower();
            lens.FocalLength = ExtractFocal(text);
            lens.SensorFormat = ExtractSensor(text);

            return lens;
        }

        private string ExtractFocal(string text)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                text, @"\d+(\.\d+)?\s?мм|\d+(\.\d+)?\s?mm"
            );
            return match.Success ? match.Value : "";
        }

        private string ExtractSensor(string text)
        {
            var match = System.Text.RegularExpressions.Regex.Match(text, @"\d+/\d+");
            return match.Success ? match.Value : "";
        }
    }
}