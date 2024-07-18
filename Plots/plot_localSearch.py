import matplotlib.pyplot as plt
from matplotlib.ticker import FuncFormatter


def parse_file(filename):
    elapsed_times = []
    costs = []
    handled_cases = []

    with open(filename, 'r') as file:
        for line in file:
            if 'UPDATE' in line:
                print(line)
                parts = line.split(',')
                elapsed = float(parts[0].split('elapsed: ')[1])
                cost = int(parts[3].split('cost: ')[1])
                handled = int(parts[6].split('handled: ')[1])

                elapsed_times.append(elapsed)
                costs.append(cost)
                handled_cases.append(handled)

    return elapsed_times, costs, handled_cases


if __name__ == "__main__":
    filename = 'LocalSearch_empty.log'
    elapsed_times, costs, handled_cases = parse_file(filename)

    # fig, ax1 = plt.subplots()
    fig, ax1 = plt.subplots(figsize=(16, 9))  # Adjust the width and height

    color = 'tab:blue'
    ax1.set_xlabel('Elapsed Time (seconds)')
    ax1.set_ylabel('Cost', color=color)
    ax1.plot(elapsed_times, costs, color=color)
    ax1.tick_params(axis='y', labelcolor=color)

    # Setting y-axis format to integer
    ax1.get_yaxis().set_major_formatter(
        FuncFormatter(lambda x, p: format(int(x), ',')))

    ax2 = ax1.twinx()
    color = 'tab:red'
    ax2.set_ylabel('Handled Incidents Count', color=color)
    ax2.plot(elapsed_times, handled_cases, color=color)
    ax2.tick_params(axis='y', labelcolor=color)

    fig.tight_layout()
    plt.title('Cost and Handled Incidents Count Over Time')
    plt.show()
