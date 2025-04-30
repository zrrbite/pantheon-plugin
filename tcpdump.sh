ssh deck@192.168.86.42 "/usr/bin/tcpdump -s 0 -U -n -w - -i wlan0 udp portrange 7000-8000" | python3.11 shark.py

#deck ALL=(ALL) !ALL
#deck ALL=NOPASSWD: TCPDUMP_WLAN0