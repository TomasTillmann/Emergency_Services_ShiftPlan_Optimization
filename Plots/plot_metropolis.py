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
plt.figure(figsize=(8, 6))
plt.plot(x, y)
plt.title(
    'Graph of Acceptance Probability of Metropolis Criterion with Temperature set to 5')
plt.xlabel('Delta')
plt.ylabel('Acceptance probability')
plt.xlim(0, 1)
plt.ylim(0, 1)
plt.grid(True)
plt.legend()
plt.show()
