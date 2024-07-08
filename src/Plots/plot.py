import matplotlib.pyplot as plt

# List of adjusted integer coordinates
int_coords = [
    (22127, 12745), (23970, 4343), (16277, 15183), (15774, 13533),
    (17008, 8144), (18013, 4684), (14022, 10176), (14447, 8711),
    (13328, 5432), (12069, 11577), (11294, 8554), (10593, 4744),
    (9740, 10804), (9571, 10533), (10472, 7969), (7913, 7448),
    (5921, 0), (3581, 5839), (2130, 12078), (0, 12908)
]

# Extract the adjusted x and y coordinates for plotting
adjusted_x_coords = [x for x, y in int_coords]
adjusted_y_coords = [y for x, y in int_coords]

# Plot the adjusted integer coordinates
plt.figure(figsize=(10, 8))
plt.scatter(adjusted_x_coords, adjusted_y_coords)
plt.xlabel('X')
plt.ylabel('Y')
plt.title('Lokace výjezdových stanic Prahy')
plt.grid(True)
plt.axis('equal')
plt.show()
