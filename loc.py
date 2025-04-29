#!/usr/bin/env python3
import time
import random
import webbrowser

LOC_FILE = "loc"
POLL_INTERVAL = 5  # seconds

def write_random_loc():
    x = random.randint(-2000, 2000)
    y = random.randint(-2000, 2000)
    with open(LOC_FILE, "w") as f:
        f.write(f"{x}.{y}")
    return x, y

def read_loc():
    try:
        with open(LOC_FILE, "r") as f:
            content = f.read().strip()
        x_str, y_str = content.split(".", 1)
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
    last = None
    while True:
        #--- test part: overwrite loc with a new random x,y ---
        x, y = write_random_loc()
        
        #--- now read and open if changed ---
        coords = read_loc()
        if coords and coords != last:
            url = build_url(*coords)
            print(f"[{time.strftime('%H:%M:%S')}] {coords} → {url}")
            webbrowser.open(url)
            last = coords

        time.sleep(POLL_INTERVAL)

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\nStopped by user")