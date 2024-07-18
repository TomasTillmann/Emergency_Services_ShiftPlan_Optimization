import numpy as np
import matplotlib.pyplot as plt

# Define the function
temp = 5


def f(x):
    return np.exp(-x / temp)


# Generate x values
x = np.linspace(0, 1, 400)

# Calculate y values
y = f(x)

# Create the plot
fig, ax = plt.subplots()
fig.set_figheight(0.1)  # Adjust the height
fig.set_figwidth(4)   # Adjust the width accordingly
ax.plot(x, y)
ax.set_title(
    'Graph of Acceptance Probability of Metropolis Criterion with Temperature set to 5')
ax.set_xlabel('Delta')
ax.set_ylabel('Acceptance probability')
ax.set_xlim(0, 1)
ax.set_ylim(0, 1)
ax.grid(True)
ax.legend(['Acceptance Probability'])
plt.show()
