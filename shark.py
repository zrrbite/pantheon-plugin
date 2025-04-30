# steamdeck allow pacman

# https://www.reddit.com/r/SteamDeck/comments/t8al0i/install_arch_packages_on_your_steam_deck/
# sudo steamos-readonly disable
# sudo pacman-key --init
# sudo pacman-key --populate archlinux
# sudo pacman-key --populate holo
#  - install something sudo pacman -S vi
# 
# capture rights for tcpdump
# sudo setcap cap_net_raw,cap_net_admin=eip $(which tcpdump)
#
#  Maybe visudo
#
#
# run with:
# ssh deck@steamdeck "tcpdump -s 0 -U -n -w - -i wlan0 udp portrange 7000-8000" | python3 shark.py
# might need scapy: pipx install scapy (pipx does not require venv setup, as opposed to pip.
# If you have/use Python venvs, and don't have pipx installed, then just install via. pip
from scapy.all import PcapReader, UDP
import struct
import sys

# location offsets in packet
FLOAT_OFFSETS = [15, 20, 25]

def parse_1a_packet(payload: bytes):
    if len(payload) < max(FLOAT_OFFSETS) + 4:
        return None  # not enough data
    if payload[0] != 0x1a:
        return None  # not a matching packet
    if len(payload) != 35: # not matching payload length
        return None

    try:
        x = struct.unpack('<f', payload[FLOAT_OFFSETS[0]:FLOAT_OFFSETS[0]+4])[0]
        y = struct.unpack('<f', payload[FLOAT_OFFSETS[1]:FLOAT_OFFSETS[1]+4])[0]
        z = struct.unpack('<f', payload[FLOAT_OFFSETS[2]:FLOAT_OFFSETS[2]+4])[0]
        return x, y, z
    except struct.error:
        return None

def main():
    reader = PcapReader(sys.stdin.buffer)
    count = 0
    for packet in reader:
        if UDP in packet:
            payload = bytes(packet[UDP].payload)
            result = parse_1a_packet(payload)
            if result:
                x, y, z = result
                print(f"Pos ({count}): ({x:.2f}, {y:.2f}, {z:.2f})")
                count = count + 1

if __name__ == "__main__":
    main()