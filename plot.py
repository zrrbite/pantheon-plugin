
#import matplotlib.pyplot as plt
#import matplotlib.image as mpimg

# Load and show image
#img = mpimg.imread('world.png')
#fig, ax = plt.subplots()
#ax.imshow(img)

# Plot on the *same axes*
#ax.axis('off')
#ax.plot([100, 200], [100, 200], 'ro-')       # red line
#ax.scatter([300], [150], color='blue')      # blue dot
#ax.text(250, 250, 'Hello!', color='white')  # text

#plt.show()

import matplotlib.pyplot as plt
import matplotlib.image as mpimg
import matplotlib.patches as patches

# --------------------------------------------------------------------------
# Defines
# --------------------------------------------------------------------------
zoom = 1
map_width, map_height = 967, 704            # in pixels. Try changing it.
world_xmin, world_xmax = -9789, 10309
world_ymin, world_ymax = -6461, 8081

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
    return round(px, 2), round(py, 2)

player_px = world_to_map((0, 0))
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

# Draw something on top
#ax.plot([0, width], [0, height], 'r-')
#ax.text(50, 50, 'Text', color='white')

# Display image dimensions in top-left corner
dim_text = f'{width} x {height}px'
ax.text(10, 20, dim_text, color='yellow', fontsize=12, backgroundcolor='black')

# Player marker (triangle)
player_marker = patches.RegularPolygon(
    player_px,        # player pos
    numVertices=3,   # triangle
    radius=8,        # make it bigger
    orientation=0,   # 0 = pointing right; use -pi/2 for upward
    color='red'
)
ax.add_patch(player_marker)

# POI
ax.scatter([20, 80], [30, 70], color='yellow', marker='o')

# Viewport rectangle
viewport = patches.Rectangle((40, 40), 20, 20, linewidth=1, edgecolor='white', facecolor='none')
ax.add_patch(viewport)

plt.show()