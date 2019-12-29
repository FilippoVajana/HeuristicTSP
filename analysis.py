import os
import glob
import matplotlib.pyplot as plt
from matplotlib.gridspec import GridSpec
import numpy as np
from tqdm import tqdm
import pandas as pd

### IO ###
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


### PLOTS ###
def get_circuit_ax(cities_pos, result, ax):
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

def get_cost_fig(i_costs, i_optimum, i_name):
    fig = plt.figure(figsize=(10,10))    

    gs = GridSpec(2, 2, width_ratios=[2, 1], height_ratios=[1, 1])

    ax1 = fig.add_subplot(gs[0, 0:1])
    plt.hist(i_costs, 15, rwidth=.8)
    plt.xlabel('Solution value')
    plt.ylabel('Count')
    plt.minorticks_on()
    plt.axvline(x=i_optimum, label=f'optimum = {i_optimum}', c='red')

    ax2 = fig.add_subplot(gs[1, 0:1])
    plt.minorticks_on()
    plt.xlabel('Relative solution quality [%]')
    plt.ylabel('Cumulative frequency')    
    i_costs.sort()
    data = np.array(i_costs)
    diff_relative = np.abs(data - i_optimum) / i_optimum * 100
    plt.plot(diff_relative, np.arange(len(diff_relative)))


    ax3 = fig.add_subplot(gs[:, -1])    
    plt.boxplot(i_costs, labels=[""])
    for c in i_costs:
        plt.scatter(x=1, y=c, c='silver')    
    
    plt.scatter(x=1, y=i_optimum, label=f'optimum = {i_optimum}', c='red')
    plt.plot([], [], label=f'median = {np.median(i_costs)}', c='orange')
    plt.plot([], [], label=f'min,max = {np.min(i_costs)},{np.max(i_costs)}')
    plt.plot([], [], label=f'Q1,Q3 = {np.quantile(i_costs, .25)},{np.quantile(i_costs, .75)}', c='black')    
    
    return fig

def boxplot_compare(costs_dict, optimus_dict):
    fig = plt.figure(figsize=(10,10))    
    fig.subplots_adjust(hspace=0.4)
    idx = 1

    for k in costs_dict.keys():
        ax = plt.subplot(3, 4, idx)
        ax.set_title(k)
        ax.set_yticklabels([])
        ax.boxplot(x=costs_dict[k], labels=["semigreedy", "grasp"])
        ax.scatter(x=[1,2], y=[optimus_dict[k], optimus_dict[k]], label=f'optimum = {optimus_dict[k]}', c='red')
        idx += 1
    
    return fig



### CSV ###
def get_rsq_table(costs_dict, optimus_dict):
    data = {        
        'best' : [],
        'worst' : [],
        'avg' : []}

    for inst in costs_dict.keys():
        # get optimum
        opt = optimus_dict[inst]
        # get min, max, avg
        best = min(costs_dict[inst], key=lambda t: t[1])[1]
        worst = max(costs_dict[inst], key=lambda t: t[1])[1]
        avg = np.mean([cost for circ, cost in costs_dict[inst]])
        # rsq        
        data['best'].append(np.abs(best - opt) / opt * 100)
        data['worst'].append(np.abs(worst - opt) / opt * 100)
        data['avg'].append(np.abs(avg - opt) / opt * 100)
        

    df = pd.DataFrame(data=data, index=costs_dict.keys())
    df = df.round(2)
    return df

def rsq_compare(costs_dict, optimus_dict):
    data = {
        'semigreedy_rsq': [],
        'grasp_rsq': [],
        'delta_rsq': []}
    
    for inst in costs_dict.keys():
        # get optimum
        opt = optimus_dict[inst]
        # get avgs        
        grasp_avg = np.mean(costs_dict[inst][1])
        semigreedy_avg = np.mean(costs_dict[inst][0])
        # rsq        
        data['semigreedy_rsq'].append(np.abs(semigreedy_avg - opt) / opt * 100)
        data['grasp_rsq'].append(np.abs(grasp_avg - opt) / opt * 100)

    # delta
    delta_arr = (1 - np.asarray(data['grasp_rsq']) / np.asarray(data['semigreedy_rsq'])) * 100
    data['delta_rsq'] = -1 * delta_arr
        

    df = pd.DataFrame(data=data, index=costs_dict.keys())
    df = df.round(2)
    return df

### HELPER ###
def get_solutions(path):
    res_files = sorted(list(filter(lambda name: '.dat' in name, os.listdir(path))))
    res_dict = dict()
    for f in tqdm(res_files, desc="Read Results"):
        try:
            res = read_results(os.path.join(path, f))
            res_dict[str(f).replace("_mat.dat.txt", "")] = res
        except Exception:
            pass

    return res_dict

def get_2dpositions(path):
    pos_files = sorted(list(filter(lambda name: '_pos.dat' in name, os.listdir(path))))
    pos_dict = dict()    
    for f in tqdm(pos_files, desc="Read Positions"):        
        pos = read_positions_file(os.path.join(POSITIONS_2D_DATA_PATH, f))
        pos_dict[str(f).replace("_pos.dat", "")] = pos
    
    return pos_dict


if __name__ == '__main__':
    POSITIONS_2D_DATA_PATH = "./data/instances"
    BENCHMARK_RESULTS_PATH = "./data/results/grasp-rcl"
    CIRCUITS_PLOT_PATH = os.path.join(BENCHMARK_RESULTS_PATH, "circuits")
    STATS_PLOT_PATH = os.path.join(BENCHMARK_RESULTS_PATH, "stats")
    OPTIMUM = {
        "att48" : 10628,
        "bayg29" : 1610,
        "bays29" : 2020,        
        "burma14" : 3323,
        "fri26" : 937,
        "gr21" : 2707,
        "gr24" : 1272,
        "pr76" : 108159,
        "st70" : 675 }

    # get nodes positions        
    pos_dict = get_2dpositions(POSITIONS_2D_DATA_PATH)

    # get solutions circuits
    res_dict = get_solutions(BENCHMARK_RESULTS_PATH)

    print("Positions Files: ", pos_dict.keys())
    print("Results Files: ", res_dict.keys())


    ### PLOT CIRCUITS
    try:
        os.makedirs(CIRCUITS_PLOT_PATH)
    except Exception:
        pass

    for k in tqdm(sorted(pos_dict.keys()), desc="Plot Circuits"):
        fig, ax = plt.subplots(nrows=1, ncols=2, figsize=(10,10))        
        plt.setp(ax, xticks=[], yticks=[])
        plt.subplots_adjust(wspace=0)

        # get best and worst circuit        
        best = min(res_dict[k], key=lambda t: t[1])
        worst = max(res_dict[k], key=lambda t: t[1])

        # plot        
        get_circuit_ax(pos_dict[k], best, ax[0])        
        get_circuit_ax(pos_dict[k], worst, ax[1])        
        plt.savefig(os.path.join(CIRCUITS_PLOT_PATH, f"{k}.png"), arr=fig, format='png', bbox_inches='tight')
    
    
    ### PLOT STATS
    try:
        os.makedirs(STATS_PLOT_PATH)
    except Exception:
        pass
    
    for k in tqdm(sorted(res_dict.keys()), desc="Plot Stats"):
        costs = [c for (circuit, c) in res_dict[k]]
        fig = get_cost_fig(costs, OPTIMUM[k], k)
        plt.savefig(os.path.join(STATS_PLOT_PATH, f"{k}.png"), arr=fig, format='png', bbox_inches='tight')


    ### RSQ
    df = get_rsq_table(res_dict, OPTIMUM)
    df.to_csv(os.path.join(BENCHMARK_RESULTS_PATH, 'rsq.csv'))

    
    ### BOXPLOT COMPARISON
    semigreedy_results_dict = get_solutions("./data/results/grasp-rcl")
    grasp_results_dict = get_solutions("./data/results/grasp")
    # merge dicts (semigreedy, grasp)
    bench_results_dict = dict()
    for k in semigreedy_results_dict.keys():
        semigreedy = [c for (p,c) in semigreedy_results_dict[k]]
        grasp = [c for (p,c) in grasp_results_dict[k]]
        bench_results_dict[k] = (semigreedy, grasp)
    # plot boxplots
    fig = boxplot_compare(bench_results_dict, OPTIMUM)
    plt.savefig(os.path.join("./data/results/grasp-rcl", "boxplot_compare.png"), arr=fig, format='png', bbox_inches='tight')


    ### RSQ COMPARISON
    semigreedy_results_dict = get_solutions("./data/results/grasp-rcl")
    grasp_results_dict = get_solutions("./data/results/grasp")
    # merge dicts (semigreedy, grasp)
    bench_results_dict = dict()
    for k in semigreedy_results_dict.keys():
        semigreedy = [c for (p,c) in semigreedy_results_dict[k]]
        grasp = [c for (p,c) in grasp_results_dict[k]]
        bench_results_dict[k] = (semigreedy, grasp)

    df = rsq_compare(bench_results_dict, OPTIMUM)
    df.to_csv(os.path.join("./data/results/grasp-rcl", 'rsq_compare.csv'))