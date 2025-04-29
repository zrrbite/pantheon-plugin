#!/usr/bin/env python3
import time, random, os
from playwright.sync_api import sync_playwright
#python3.11 -m pip install playwright
LOC_FILE      = "loc"
POLL_INTERVAL = 5      # seconds
X_RANGE       = (-2000, 2000)
Y_RANGE       = (-2000, 2000)

def write_random_loc():
    x = random.randint(*X_RANGE)
    y = random.randint(*Y_RANGE)
    with open(LOC_FILE, "w") as f:
        f.write(f"{x}.{y}")
    return x, y

def read_loc():
    try:
        with open(LOC_FILE, "r") as f:
            x_str, y_str = f.read().strip().split(".", 1)
        return int(x_str), int(y_str)
    except Exception:
        return None

def build_url(x, y):
    return (
        f"https://shalazam.info/maps/1"
        f"?zoom=7&x={x}&y={y}"
        f"&pin%5B%5D={x}.{y}.Dropped+Pin"
    )

def main():
    # launch one browser + tab
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=False)
        page    = browser.new_page()
        page.goto("about:blank")

        last = None
        try:
            while True:
                # 1) write new random loc
                write_random_loc()

                # 2) read & open if changed
                coords = read_loc()
                if coords and coords != last:
                    url = build_url(*coords)
                    print(f"[{time.strftime('%H:%M:%S')}] → {coords} → {url}")
                    page.goto(url)     # <— reuses the same tab!
                    last = coords

                time.sleep(POLL_INTERVAL)

        except KeyboardInterrupt:
            print("\nStopping…")
        finally:
            browser.close()

if __name__ == "__main__":
    main()