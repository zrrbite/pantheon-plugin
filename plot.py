import matplotlib.pyplot as plt
import matplotlib.image as mpimg
import matplotlib.patches as patches
from math import pi

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

# player_px = world_to_map((-1418, 7405))
player_px = world_to_map((3340, -2600))
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
# Player marker (triangle)
marker_radius = 8
player_marker = patches.RegularPolygon(
    (player_px[0], player_px[1] - marker_radius),  # shift up
    numVertices=3,
    radius=marker_radius,
    orientation=-pi/2,  # triangle tip pointing up
    color='red'
)
ax.add_patch(player_marker)

plt.show()