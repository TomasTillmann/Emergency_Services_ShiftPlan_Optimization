import matplotlib.pyplot as plt

# Data
elapsed = [458.6331705, 635.9802923, 802.909923, 934.7650236, 1057.6484564,
           1167.325766, 1280.0806583, 1385.2753851, 1491.2216149, 1587.6098005, 1690.4004763]
cost = [2520055, 2325655, 2412060, 2455258, 2325654,
        2505663, 2340053, 2390458, 2556061, 2325655, 2325653]
handled = [295, 292, 294, 295, 288, 295, 288, 292, 295, 290, 286]

# Create a figure and axis
fig, ax1 = plt.subplots()

# Plot cost
ax1.plot(elapsed, cost, 'b-', label='Cost')
ax1.set_xlabel('Elapsed Time')
ax1.set_ylabel('Cost', color='b')
ax1.tick_params(axis='y', labelcolor='b')

# Create a second y-axis to plot handled
ax2 = ax1.twinx()
ax2.plot(elapsed, handled, 'r-', label='Handled')
ax2.set_ylabel('Handled', color='r')
ax2.tick_params(axis='y', labelcolor='r')

# Add a title
plt.title('Cost and Handled over Time')

# Add a legend
fig.tight_layout()  # To ensure the layout is tight and nothing is cut off
fig.legend(loc="upper right", bbox_to_anchor=(
    1, 1), bbox_transform=ax1.transAxes)

# Show the plot
plt.show()
