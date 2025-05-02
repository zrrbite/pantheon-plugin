import matplotlib.pyplot as plt
import matplotlib.image as mpimg
import matplotlib.patches as patches
from math import pi, cos, sin
from scapy.all import PcapReader, UDP
import struct
import sys

# location offsets in packet
FLOAT_OFFSETS = [15, 20, 25]

# --------------------------------------------------------------------------
# Defines
# --------------------------------------------------------------------------
dx = 194 - 365
dy = 458 - 428
calibration_offset = (dx, dy)
#calibration_offset = (-10.5, 42.0) # shalazam 0,0 = 473, 394. Mapgate: (483.5, 352.0).   shala 0,0 194, 458. ours 354 469
zoom = 1
map_width, map_height = 728, 862 #967, 704            # in pixels. Try changing it.
world_xmin, world_xmax = -1969, 5265 #-9789, 10309
world_ymin, world_ymax = -4186, 4673 #-6461, 8081

world_width = world_xmax - world_xmin
world_height = world_ymax - world_ymin

origin_px = (map_width/2, map_height/2)     # Pixel location of (0,0)

pixels_per_unit_x = (map_width * zoom) / world_width
pixels_per_unit_y = (map_height * zoom) / world_height

print("pixels per unit {}".format(pixels_per_unit_x))
print("pixels per unit {}".format(pixels_per_unit_y))

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

def draw_player_circle_with_heading(ax, center, heading_rad, radius=5, line_length=12, color='red'):
    from math import cos, sin

    cx, cy = center

    # Draw circle at player position
    circle = plt.Circle(
        (cx, cy - radius/2),
        radius,
        edgecolor=color,
        facecolor='red',
        linewidth=2)
    ax.add_patch(circle)

    # Draw heading line
    dx = cos(heading_rad) * line_length
    dy = -sin(heading_rad) * line_length  # negative for screen y-down
    ax.plot([cx, cx + dx], [cy - radius/2, cy + dy], color=color, linewidth=2)

# Draw player arrow at world position
def draw_player_arrow(ax, tip, heading_rad, length=15, width=3, color='red'):
    tx, ty = tip
    dx = cos(heading_rad) * length
    dy = -sin(heading_rad) * length  # account for y-down screen

    start_x = tx - dx
    start_y = ty - dy

    arrow = patches.FancyArrow(
        start_x, start_y, dx, dy,
        width=width,
        head_width=width * 1.5,
        head_length=length * 0.5,
        length_includes_head=True,
        color=color
    )
    ax.add_patch(arrow)

# --------------------------------------------------------------------------
# Transform world scale to map scale
#    Focus on center of map ( origin_px ). But, we COULD change origin_px to be the players location! e.g. Minimap-style
# --------------------------------------------------------------------------
def world_to_map(world_pos):
    x, y = world_pos
    px = origin_px[0] + x * pixels_per_unit_x
    py = origin_px[1] - y * pixels_per_unit_y
    
    # Apply calibration
    px += calibration_offset[0]
    py += calibration_offset[1]

    return round(px, 2), round(py, 2)

def main():
    #set up canvas
    #player_px = world_to_map((3340, -2600))
    player_px = world_to_map((3240, 4268))

    print(player_px)

    img = mpimg.imread('world.png')
    height, width = img.shape[:2]

    # Set figure size to match image exactly
    dpi = 100
    fig = plt.figure(figsize=(width / dpi, height / dpi), dpi=dpi)
    ax = fig.add_axes([0, 0, 1, 1])  # remove all padding

    # Show image
    ax.imshow(img, extent=[0, width, height, 0])
    ax.axis('off')

    # Display image dimensions in top-left corner
    dim_text = f'{width} x {height}px'
    ax.text(10, 20, dim_text, color='yellow', fontsize=12, backgroundcolor='black')

    """
    heading = your_heading_in_radians  # 0 = right, π/2 = up, etc.
    marker_radius = 8

    # Shift center backward so tip aligns with position
    dx = -marker_radius * np.cos(heading)
    dy = -marker_radius * np.sin(heading)

    adjusted_px = (player_px[0] + dx, player_px[1] + dy)

    player_marker = patches.RegularPolygon(
        adjusted_px,
        numVertices=3,
        radius=marker_radius,
        orientation=heading,
        color='red'
    )
    """

    # Set a test heading
    heading = -pi / 2  # up
    #draw_player_arrow(ax, player_px, heading)
    draw_player_circle_with_heading(ax, player_px, heading)
    tet_loc = world_to_map((0, 0))
    ax.text(tet_loc[0]-50, tet_loc[1]-80, "World Origin", color='white', fontsize=12, backgroundcolor='black')

    plt.show(block=False)
    plt.pause(0.1)

    reader = PcapReader(sys.stdin.buffer)
    count = 0
    for packet in reader:
        if UDP in packet:
            plt.pause(1.0)
            payload = bytes(packet[UDP].payload)
            result = parse_1a_packet(payload)
            if result:
                x, y, z = result
                print(f"Pos ({count}): ({x:.2f}, {y:.2f}, {z:.2f})")
                print(f"Pos ({count}): ({x}, {y}, {z:.2f})")
                
                draw_player_circle_with_heading(ax, world_to_map((int(x), int(z))), heading)
                count = count + 1

if __name__ == "__main__":
    main()