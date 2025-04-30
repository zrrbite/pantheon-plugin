# Stationary Map centered at world origin (0,0)

# --------------------------------------------------------------------------
# Defines
# --------------------------------------------------------------------------
zoom = 1
map_width, map_height = 512, 512            # in pixels. Try changing it.
world_max = 10000                           # world size in coordinates
world_min = -10000                          # world size in coordinates
origin_px = (map_width/2, map_height/2)     # Pixel location of (0,0)
pixels_per_unit = (map_width * zoom) / (world_max - (world_min)); # map width / world width = 0.0256
print("pixels per unit {}".format(pixels_per_unit))

# --------------------------------------------------------------------------
# Transform world scale to map scale
#    Focus on center of map ( origin_px ). But, we COULD change origin_px to be the players location! e.g. Minimap-style
# --------------------------------------------------------------------------
def world_to_map(world_pos):
    x, y = world_pos
    px = origin_px[0] + x * pixels_per_unit
    py = origin_px[1] - y * pixels_per_unit
    return round(px, 2), round(py, 2)

# -----------------------------------------------------------
# Example usage:
# -----------------------------------------------------------
#  The last sample point will always be close to the edge of the map for zoom level 1 and wont change much regardless of world size. Try changing map_width and map_height
#  If you choose zoom 2, the last sample point will be off-screen, since we're zooming on world origin. If we changed world origin to be player origin we would get positive values, and we would have to pan the map. Thats a todo. 
#
# Output: zoom level 1:
#  pixels per unit 0.0256
#  World (-1000, 1000) => Map Pixel (230.4, 230.4)
#  World (3264, 521) => Map Pixel (339.56, 242.66)
#  World (0, 0) => Map Pixel (256.0, 256.0)
#  World (-9000, 10000) => Map Pixel (25.6, 0.0)
#
# Output: zoom level 2:
#  pixels per unit 0.0512
#  World (-1000, 1000) => Map Pixel (204.8, 204.8)
#  World (3264, 521) => Map Pixel (423.12, 229.32)
#  World (0, 0) => Map Pixel (256.0, 256.0) // always center, regardless of zoom level. IF we're zooming on origin
#  World (-9000, 10000) => Map Pixel (-204.8, -256.0)
# -------------------------------------------------------------------
sample_points = [(-1000, 1000), (3264, 521), (0, 0), (-9000, 10000)]
for pt in sample_points:
    print(f"World {pt} => Map Pixel {world_to_map(pt)}")
