import asyncio
import aiohttp
from bs4 import BeautifulSoup
import json

BASE_URL = "https://azimp.ru"
SEARCH_URL = "https://azimp.ru/catalogue/index.php?q=объектив&PAGEN_1={}"

HEADERS = {"User-Agent": "Mozilla/5.0"}

MAX_CONCURRENT = 10  # небольшое значение, чтобы не перегружать сайт
progress = 0
TOTAL = 0

# -------------------------
# 🔹 Универсальный fetch с retry
# -------------------------
async def fetch(session, url):
    for attempt in range(3):
        try:
            async with session.get(url, headers=HEADERS) as resp:
                return await resp.text()
        except Exception:
            await asyncio.sleep(1 * (attempt + 1))
    return None

# -------------------------
# 🔹 Парсим страницу поиска
# -------------------------
async def parse_search_page(session, page):
    url = SEARCH_URL.format(page)
    text = await fetch(session, url)
    if not text:
        return []

    soup = BeautifulSoup(text, "html.parser")
    items = soup.select("div.list_item_wrapp a.thumb")
    links = [BASE_URL + a.get("href") for a in items if a.get("href")]
    print(f"Страница {page}: {len(links)} товаров")
    return links

# -------------------------
# 🔹 Парсим один товар
# -------------------------
async def parse_product(session, url, sem):
    global progress
    async with sem:
        text = await fetch(session, url)
    if not text:
        return None
    try:
        soup = BeautifulSoup(text, "html.parser")
        title_el = soup.select_one("h1")
        title = title_el.text.strip() if title_el else ""

        characteristics = {}
        for p in soup.select(".properties__item"):
            name_el = p.select_one(".properties__title")
            value_el = p.select_one(".properties__value")
            if name_el and value_el:
                characteristics[name_el.text.strip()] = value_el.text.strip()

        progress += 1
        print(f"Прогресс: {progress}")

        return {"title": title, "link": url, "characteristics": characteristics}
    except Exception:
        return None

# -------------------------
# 🔹 Главный запуск
# -------------------------
async def main():
    global TOTAL
    results = []

    connector = aiohttp.TCPConnector(limit=MAX_CONCURRENT, ttl_dns_cache=300)
    timeout = aiohttp.ClientTimeout(total=30)

    sem = asyncio.Semaphore(MAX_CONCURRENT)

    async with aiohttp.ClientSession(connector=connector, timeout=timeout) as session:

        # Последовательно идём по страницам
        for page in range(1, 51):  # всего 50 страниц
            links = await parse_search_page(session, page)
            TOTAL += len(links)

            # Парсим товары на странице последовательно через Semaphore
            for url in links:
                product = await parse_product(session, url, sem)
                if product:
                    results.append(product)

            # Сохраняем после каждой страницы (чтобы не потерять данные)
            with open("azimp_lenses.json", "w", encoding="utf-8") as f:
                json.dump(results, f, ensure_ascii=False, indent=2)

        print(f"Готово! Всего товаров: {len(results)}")
        print("Файл сохранен: azimp_lenses.json")

if __name__ == "__main__":
    asyncio.run(main())