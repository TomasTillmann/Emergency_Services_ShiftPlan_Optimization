import matplotlib.pyplot as plt

# Function to parse the file


def parse_file(filename):
    elapsed_times = []
    costs = []
    handled_cases = []
    evals = []

    with open(filename, 'r') as file:
        for line in file:
            if 'UPDATE' in line:
                parts = line.split(',')
                elapsed = float(parts[0].split('elapsed: ')[1])
                cost = int(parts[1].split('cost: ')[1])
                handled = int(parts[4].split('handled: ')[1])

                elapsed_times.append(elapsed)
                costs.append(cost)
                handled_cases.append(handled)

    return elapsed_times, costs, handled_cases, evals

# Plotting function


def plot_data(elapsed_times, costs, handled_cases, evals):
    fig, ax1 = plt.subplots()

    color = 'tab:blue'
    ax1.set_xlabel('Elapsed Time (seconds)')
    ax1.set_ylabel('Cost', color=color)
    ax1.plot(elapsed_times, costs, color=color)
    ax1.tick_params(axis='y', labelcolor=color)

    ax2 = ax1.twinx()
    color = 'tab:red'
    ax2.set_ylabel('Handled Incidents Count', color=color)
    ax2.plot(elapsed_times, handled_cases, color=color)
    ax2.tick_params(axis='y', labelcolor=color)

    fig.tight_layout()
    plt.title('Cost and Handled Incidents Count Over Time')
    plt.show()


# Main script
if __name__ == "__main__":
    filename = 'hybrid_tabu.log'
    elapsed_times, costs, handled_cases, evals = parse_file(filename)
    plot_data(elapsed_times, costs, handled_cases, evals)
