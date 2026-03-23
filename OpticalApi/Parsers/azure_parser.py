import requests
from bs4 import BeautifulSoup
import json
import time

BASE_URL = "https://cameralab.ru"
START_URL = "https://cameralab.ru/catalog/obektivy-azure/"

headers = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36"
}

# 🔹 Получаем страницу каталога
r = requests.get(START_URL, headers=headers)
soup = BeautifulSoup(r.text, "html.parser")

# 🔹 Сбор категорий
category_links = set()
for a in soup.find_all("a", href=True):
    href = a['href']
    if "/catalog/obektivy-azure/" in href and href != START_URL:
        full_url = BASE_URL + href if href.startswith("/") else href
        category_links.add(full_url)

category_links = list(category_links)
print("Категорий найдено:", len(category_links))

all_products = []

# 🔹 Проходим по категориям
for url in category_links:
    print("\nПарсим категорию:", url)
    r = requests.get(url, headers=headers)
    soup = BeautifulSoup(r.text, "html.parser")

    products = soup.select(".catalog-block__item")
    print("Найдено товаров:", len(products))

    for p in products:
        try:
            title_el = p.select_one(".catalog-block__info-title a")
            title = title_el.text.strip()
            link = BASE_URL + title_el['href'] if title_el['href'].startswith("/") else title_el['href']
        except:
            title = None
            link = None

        try:
            price_el = p.select_one(".price__new-val")
            price = price_el.text.strip() if price_el else None
        except:
            price = None

        # 🔹 Получаем характеристики с детальной страницы
        characteristics = {}
        if link:
            r2 = requests.get(link, headers=headers)
            soup2 = BeautifulSoup(r2.text, "html.parser")
            table = soup2.select_one("tbody.block-wo-title.js-offers-prop")
            if table:
                for row in table.select("tr"):
                    name_el = row.select_one("span[itemprop='name']")
                    value_el = row.select_one("span[itemprop='value']")
                    if name_el and value_el:
                        name = name_el.get_text(strip=True)
                        value = value_el.get_text(strip=True)
                        characteristics[name] = value
            # Немного паузы, чтобы сайт не заблокировал
            time.sleep(0.5)

        all_products.append({
            "title": title,
            "price": price,
            "link": link,
            "characteristics": characteristics
        })

# 🔹 Убираем дубли
unique_products = []
seen_links = set()
for item in all_products:
    if item["link"] not in seen_links:
        seen_links.add(item["link"])
        unique_products.append(item)

print("\nВсего товаров:", len(all_products))
print("Уникальных товаров:", len(unique_products))

# 🔹 Сохраняем в JSON
with open("azure_lenses.json", "w", encoding="utf-8") as f:
    json.dump(unique_products, f, ensure_ascii=False, indent=4)

print("Данные сохранены в azure_lenses.json")