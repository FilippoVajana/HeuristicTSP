import os
import glob
import matplotlib.pyplot as plt
from matplotlib.gridspec import GridSpec
import numpy as np

def read_positions_file(file_path : str):
    positions = dict()
    # parse file
    file = open(file_path, 'r')    
    for line in file:
        line = str(line).replace('\n', '')
        split = [float(s) for s in line.split(' ') if s != '']
        positions[int(split[0] - 1)] = (split[1], split[2])
    
    return positions # {Node, (X,Y)}

def read_results(file_path : str):
    results = []
    # parse file
    file = open(file_path, 'r')
    lines = file.readlines()
    for i in range(0, len(lines), 2):
        circuit = [int(id) for id in lines[i].replace('\n', '').split(' ')]
        cost = int(lines[i+1].replace('\n', ''))
        results.append((circuit, cost))
    
    return results # [(circuit, cost)]

def plot_circuit(cities_pos, result, ax):
    circuit, cost = result
    # get cities coordinates
    pos = [v for v in cities_pos.values()]
    x = [p[0] for p in pos]
    y = [p[1] for p in pos] 

    # plot cities
    sc = ax.scatter(x,y)
    
    for idx, p in enumerate(pos):        
        x.append(p[0])
        y.append(p[1])
        if idx == 0:       
            ax.annotate(f"{idx} [start]", xy=(p[0],p[1]), xytext=(p[0],p[1]))
        else:
            ax.annotate(f"{idx}", xy=(p[0],p[1]), xytext=(p[0],p[1])) 

    # plot circuit
    for idx, node in enumerate(circuit):
        end = cities_pos[node]
        start = cities_pos[circuit[idx + 1]] if (idx < len(circuit) - 1) else cities_pos[circuit[0]]
        ax.annotate("", xy=start, xycoords='data', xytext=end, textcoords='data', arrowprops=dict(arrowstyle="->", connectionstyle="arc3"))    
    
    return ax

def plot_instance_circuits(name : str, node_pos, results):
    pass


def plot_costs(costs, optimum, instance_name):      
    fig = plt.figure(figsize=(10,10))
    fig.suptitle(instance_name, size=22)

    ax1 = plt.subplot(321)
    plt.hist(costs, 15, rwidth=.8)
    plt.xlabel('Solution value')
    plt.ylabel('Count')
    plt.minorticks_on()
    plt.axvline(x=optimum, label=f'optimum = {optimum}', c='red')
    plt.legend(frameon=False)

    ax2 = plt.subplot(322)
    plt.minorticks_on()
    plt.xlabel('Relative solution quality [%]')
    plt.ylabel('Cumulative frequency')
    costs.sort()
    data = np.array(costs)
    diff_relative = np.abs(data - optimum) / optimum    
    plt.plot(diff_relative, np.arange(len(diff_relative)))

    ax3 = plt.subplot(323)
    plt.boxplot(costs, vert=False)
    for c in costs:
        plt.scatter(x=c, y=1, c='silver')
    plt.ylabel('Solution value')
    
    plt.scatter(x=optimum, y=1, label=f'optimum = {optimum}', c='red')
    plt.plot([], [], label=f'median = {np.median(costs)}', c='orange')
    plt.plot([], [], label=f'min,max = {np.min(costs)},{np.max(costs)}')
    plt.plot([], [], label=f'Q1,Q3 = {np.quantile(costs, .25)},{np.quantile(costs, .75)}', c='black')    
    
    plt.legend(frameon=False)


def plot_costs2(costs, optimum, instance_name):
    fig = plt.figure(figsize=(10,10))
    # fig.suptitle(instance_name, size=22)

    gs = GridSpec(2, 2, width_ratios=[2, 1], height_ratios=[1, 1])

    ax1 = fig.add_subplot(gs[0, 0:1])
    plt.hist(costs, 15, rwidth=.8)
    plt.xlabel('Solution value')
    plt.ylabel('Count')
    plt.minorticks_on()
    plt.axvline(x=optimum, label=f'optimum = {optimum}', c='red')

    ax2 = fig.add_subplot(gs[1, 0:1])
    plt.minorticks_on()
    plt.xlabel('Relative solution quality [%]')
    plt.ylabel('Cumulative frequency')    
    costs.sort()
    data = np.array(costs)
    diff_relative = np.abs(data - optimum) / optimum * 100
    plt.plot(diff_relative, np.arange(len(diff_relative)))


    ax3 = fig.add_subplot(gs[:, -1])    
    plt.boxplot(costs, labels=[""])
    for c in costs:
        plt.scatter(x=1, y=c, c='silver')    
    
    plt.scatter(x=1, y=optimum, label=f'optimum = {optimum}', c='red')
    plt.plot([], [], label=f'median = {np.median(costs)}', c='orange')
    plt.plot([], [], label=f'min,max = {np.min(costs)},{np.max(costs)}')
    plt.plot([], [], label=f'Q1,Q3 = {np.quantile(costs, .25)},{np.quantile(costs, .75)}', c='black')    
    
    return fig





if __name__ == '__main__':
    # get nodes positions
    pos_root = "./data/instances"
    pos_files = sorted(list(filter(lambda name: '_pos.dat' in name, os.listdir(pos_root))))
    pos_dict = dict()
    
    for f in pos_files:        
        pos = read_positions_file(os.path.join(pos_root, f))
        pos_dict[str(f).replace("_pos.dat", "")] = pos


    # get solutions circuits
    res_root = "./data/results/6023"
    res_files = sorted(os.listdir(res_root))
    res_dict = dict()

    for f in res_files:             
        res = read_results(os.path.join(res_root, f))
        res_dict[str(f).replace("_mat.dat.txt", "")] = res

    print("Positions Files: ", pos_dict.keys())
    print("Results Files: ", res_dict.keys())


    # plot circuits
    # for k in sorted(pos_dict.keys()): 
    #     fig, ax = plt.subplots(nrows=1, ncols=2)
    #     fig.suptitle(k, size=22)
    #     plt.setp(ax, xticks=[], yticks=[])
    #     plt.subplots_adjust(wspace=0)

    #     # get best and worst circuit        
    #     best = min(res_dict[k], key=lambda t: t[1])
    #     worst = max(res_dict[k], key=lambda t: t[1])

    #     # plot        
    #     plot_circuit(pos_dict[k], best, ax[0])
    #     ax[0].set_title(f"value: {best[1]}")
    #     plot_circuit(pos_dict[k], worst, ax[1])
    #     ax[1].set_title(f"value: {worst[1]}")
    # plt.show()
    
    # plot costs
    opt = {
        "att48" : 10628,
        "bayg29" : 1610,
        "bays29" : 2020,        
        "burma14" : 3323,
        "fri26" : 937,
        "gr21" : 2707,
        "gr24" : 1272,
        "pr76" : 108159,
        "st70" : 675 }

    for k in sorted(res_dict.keys()):
        costs = [c for (circuit, c) in res_dict[k]]
        fig = plot_costs2(costs, opt[k], k)
        plt.savefig(f"./data/results/plots/{k}.png", arr=fig, format='png')
        # plt.show()
        
    # plt.show()
