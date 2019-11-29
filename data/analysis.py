import os
import glob
import matplotlib.pyplot as plt
import numpy as np

def read_cities_pos(file_path : str):
    cities_pos_map = dict()
    # parse file
    file = open(file_path, 'r')    
    for line in file:
        line = str(line).replace('\n', '')
        split = [float(s) for s in line.split(' ') if s != '']
        cities_pos_map[int(split[0] - 1)] = (split[1], split[2])
    
    return cities_pos_map

def read_results(file_path : str):
    results = []
    # parse file
    file = open(file_path, 'r')
    lines = file.readlines()
    for i in range(0, len(lines), 2):
        circuit = [int(id) for id in lines[i].replace('\n', '').split(' ')]
        cost = int(lines[i+1].replace('\n', ''))
        results.append((circuit, cost))
    
    return results

def plot(cities_pos, result, ax):
    circuit, cost = result
    # get cities coordinates
    pos = [v for v in cities_pos.values()]
    x = [p[0] for p in pos]
    y = [p[1] for p in pos] 

    # plot cities    
    # _, ax = plt.subplots()    
    sc = ax.scatter(x,y) 
    for idx, p in enumerate(pos):        
        x.append(p[0])
        y.append(p[1])
        if idx == 0:       
            ax.annotate(f"{idx} [start]", xy=(p[0],p[1]), xytext=(p[0],p[1] + 25))
        else:
            ax.annotate(f"{idx}", xy=(p[0],p[1]), xytext=(p[0],p[1] + 25)) 

    # plot circuit
    for idx, node in enumerate(circuit):
        end = cities_pos[node]
        start = cities_pos[circuit[idx + 1]] if (idx < len(circuit) - 1) else cities_pos[circuit[0]]
        ax.annotate("", xy=start, xycoords='data', xytext=end, textcoords='data', arrowprops=dict(arrowstyle="->", connectionstyle="arc3"))
    
    plt.draw()
    return ax


if __name__ == '__main__':
    print(os.getcwd())
    cities_pos = read_cities_pos("bayg29_pos.dat")

    files = [os.path.normpath(f) for f in glob.glob("results/" + "*.txt", recursive=False)]
    files.sort()
    results = read_results(files[-1])

    best_res = min(results, key=lambda t: t[1])
    best_res_idx = results.index(best_res)
    worst_res = max(results, key=lambda t: t[1])
    worst_res_idx = results.index(worst_res)

    print((best_res, best_res_idx))
    print((worst_res, worst_res_idx))

    _, ax = plt.subplots(1,2, False, True)
    ax[0].set_title("Best Result")
    ax[1].set_title("Worst Result")
    best_ax = plot(cities_pos, results[best_res_idx], ax[0])
    worst_ax = plot(cities_pos, results[worst_res_idx], ax[1])

    plt.show()
