import asyncio
from playwright.async_api import async_playwright, expect

async def run():
    async with async_playwright() as p:
        browser = await p.chromium.launch(headless=True)
        context = await browser.new_context(ignore_https_errors=True)
        page = await context.new_page()

        # Navigate to the Weather page
        # Using port 5032 as seen in curl output and logs
        try:
            await page.goto("http://localhost:5032/weather", timeout=60000)

            # Wait for the table to appear (data loading)
            await expect(page.locator("table")).to_be_visible(timeout=10000)

            # Verify the headers
            await expect(page.get_by_role("columnheader", name="Temp. (C)")).to_be_visible()
            await expect(page.get_by_role("columnheader", name="Temp. (F)")).to_be_visible()

            # Verify we have 5 rows of data (ForecastDays constant)
            rows = page.locator("tbody tr")
            await expect(rows).to_have_count(5)

            # Take a screenshot
            await page.screenshot(path="verification/weather_page.png", full_page=True)
            print("Screenshot saved to verification/weather_page.png")

        except Exception as e:
            print(f"Error: {e}")
            # Take a screenshot even on error if possible
            try:
                await page.screenshot(path="verification/error_screenshot.png")
            except:
                pass
        finally:
            await browser.close()

if __name__ == "__main__":
    asyncio.run(run())
