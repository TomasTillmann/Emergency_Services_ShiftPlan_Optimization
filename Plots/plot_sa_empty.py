import matplotlib.pyplot as plt
from matplotlib.ticker import FuncFormatter

# Function to parse the file


def parse_file(filename):
    elapsed_times = []
    temps = []
    costs = []
    handled_cases = []
    evals = []

    with open(filename, 'r') as file:
        for line in file:
            if 'X' in line or 'UPDATE' in line:
                parts = line.split(',')
                elapsed = float(parts[0].split('elapsed: ')[1])
                # temp = float(parts[7].split('temp: ')[1])
                cost = int(parts[1].split('cost: ')[1])
                handled = int(parts[4].split('handled: ')[1])
                eval_value = float(parts[5].split('eval: ')[1])

                elapsed_times.append(elapsed)
                # temps += [temp]
                costs.append(cost)
                handled_cases.append(handled)
                evals.append(eval_value)

    return elapsed_times, costs, handled_cases, evals

# Plotting function


def plot_data(elapsed, costs, handled_cases, evals):
    fig, ax1 = plt.subplots()
    # Reverse the lists to have temperature decreasing
    # temps = temps[::-1]
    # costs = costs[::-1]
    # handled_cases = handled_cases[::-1]

    color = 'tab:blue'
    ax1.set_xlabel('Elapsed Time (seconds)')
    ax1.set_ylabel('Cost', color=color)
    ax1.plot(elapsed, costs, color=color)
    ax1.tick_params(axis='y', labelcolor=color)
    # ax1.set_xlim(max(temps), min(temps))

    # Setting y-axis format to integer
    ax1.get_yaxis().set_major_formatter(
        FuncFormatter(lambda x, p: format(int(x), ',')))

    ax2 = ax1.twinx()
    color = 'tab:red'
    ax2.set_ylabel('Handled Incidents Count', color=color)
    ax2.plot(elapsed, handled_cases, color=color)
    ax2.tick_params(axis='y', labelcolor=color)

    fig.tight_layout()
    plt.title('Cost and Handled Incidents Count Over Time')
    plt.show()


# Main script
if __name__ == "__main__":
    filename = 'SimulatedAnnealing_5_0000001_1_exp99_fromEmpty.log'
    elapsed, costs, handled_cases, evals = parse_file(filename)
    plot_data(elapsed, costs, handled_cases, evals)
