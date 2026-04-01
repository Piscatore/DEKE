# Neuroevolution

Neuroevolution applies evolutionary algorithms to the design and optimization of neural networks. Rather than relying solely on gradient-based training, neuroevolution searches over network topologies, weights, hyperparameters, or even learning rules using population-based methods inspired by natural selection.

This document surveys the foundational papers, available frameworks, core concepts, and practical considerations for researchers and engineers approaching the field.

## Key Papers

### NEAT — NeuroEvolution of Augmenting Topologies

**Stanley, K. O. & Miikkulainen, R. (2002).** "Evolving Neural Networks through Augmenting Topologies." *Evolutionary Computation*, 10(2), 99--127.

NEAT introduced three innovations that became standard in topology-evolving neuroevolution: (1) historical markings (innovation numbers) that enable meaningful crossover between networks of different topologies, (2) speciation to protect structural innovations during their initial low-fitness period, and (3) complexification from minimal starting structures rather than pruning from large ones. NEAT demonstrated that topology and weights could be co-evolved efficiently.

### AutoML-Zero

**Real, E., Liang, C., So, D. R., & Le, Q. V. (2020).** "AutoML-Zero: Evolving Machine Learning Algorithms From Scratch." *Proceedings of ICML 2020*.

AutoML-Zero pushed the boundary of what evolution can discover by starting from an empty program (basic math operations only) and evolving complete learning algorithms. The system rediscovered backpropagation-like gradient descent, learning rate schedules, and other techniques with no human-designed components in the search space. This demonstrated that evolutionary search can operate at a higher level of abstraction than weight or topology optimization alone.

### Evolution Strategies as a Scalable Alternative to Reinforcement Learning

**Salimans, T., Ho, J., Chen, X., Sidor, S., & Sutskever, I. (2017).** "Evolution Strategies as a Scalable Alternative to Reinforcement Learning." *arXiv:1703.03864*.

OpenAI showed that simple evolution strategies (ES) — specifically, natural evolution strategies using Gaussian perturbations — could solve reinforcement learning benchmarks (Atari, MuJoCo) competitively with policy gradient methods. ES are embarrassingly parallel, require no backpropagation, and are invariant to action frequency and delayed rewards. The paper renewed interest in black-box optimization for RL-scale problems.

### Population Based Training of Neural Networks

**Jaderberg, M., Dalibard, V., Osindero, S., Czarnecki, W. M., Donahue, J., Razavi, A., Vinyals, O., Green, T., Dunning, I., Simonyan, K., Fernando, C., & Kavukcuoglu, K. (2017).** "Population Based Training of Neural Networks." *arXiv:1711.09846*.

PBT combines random hyperparameter search with online model selection. A population of models trains in parallel; periodically, underperforming members copy the weights and hyperparameters of better-performing members (exploit) and then perturb those hyperparameters (explore). PBT adapts hyperparameters during training rather than treating them as fixed, yielding schedules that no static configuration could match.

### Self-Assembling Neural Networks

**Plantec, E., Hamon, G., Etcheverry, M., Oudeyer, P.-Y., Moulin-Frier, C., & Chan, B. W.-C. (2024).** "Growing Artificial Neural Networks with Self-Assembling Processes." Presented at ALIFE 2024.

This work applies developmental (morphogenetic) processes to neural network construction. Instead of evolving a network directly, the approach evolves the developmental program — a set of local growth rules — that constructs the network. This mirrors biological development where genotype-to-phenotype mapping is indirect and allows compact genomes to produce complex structures. Self-assembling networks exhibit properties like modularity and robustness to damage.

### Evolution Strategies at Scale

**arXiv:2509.24372 (2025).** "Evolution Strategies at Scale."

This paper extends the OpenAI 2017 ES work to modern scales, demonstrating that evolution strategies remain competitive when applied to large models and distributed across thousands of workers. It addresses variance reduction techniques, structured exploration, and communication-efficient distributed ES. The results reinforce ES as a viable optimization paradigm for problems where gradient information is unavailable or unreliable.

## Frameworks and Libraries

### NEAT-Python

A pure Python implementation of the NEAT algorithm. Suitable for research prototyping and small-to-medium experiments. Supports speciation, novelty search, and configurable reproduction operators.

- Repository: [https://github.com/CodeReclworker/neat-python](https://github.com/CodeReclworker/neat-python)

### SharpNEAT

A high-performance .NET implementation of NEAT and its variants (HyperNEAT, ES-HyperNEAT). SharpNEAT is well-suited for integration into .NET ecosystems and supports configurable activation functions, speciation strategies, and large population sizes.

- Repository: [https://github.com/colgreen/sharpneat](https://github.com/colgreen/sharpneat)
- Author: Colin Green

### EvoJAX

A hardware-accelerated neuroevolution library built on JAX. EvoJAX leverages GPU/TPU parallelism for population evaluation, making it practical to run large-population ES and genetic algorithm experiments at scale.

- Repository: [https://github.com/google/evojax](https://github.com/google/evojax)
- Reference: Tang, Y. et al. (2022). "EvoJAX: Hardware-Accelerated Neuroevolution." *arXiv:2202.05008*.

### AutoKeras

An automated machine learning (AutoML) library built on Keras/TensorFlow. While not strictly neuroevolution, AutoKeras uses search algorithms (including evolutionary methods) to discover neural architectures. It represents the AutoML end of the spectrum where architecture search is the goal.

- Repository: [https://github.com/keras-team/autokeras](https://github.com/keras-team/autokeras)
- Reference: Jin, H., Song, Q., & Hu, X. (2019). "Auto-Keras: An Efficient Neural Architecture Search System." *KDD 2019*.

## Key Concepts

### Topology Evolution

Classical neural network training optimizes weights within a fixed architecture. Topology evolution adds or removes neurons, connections, and layers during the search process. This allows the algorithm to discover architectures suited to the problem rather than requiring the researcher to specify them in advance. NEAT's complexification approach — starting minimal and growing — avoids the curse of dimensionality that afflicts searches over large fixed-topology spaces.

### Black-Box Optimization

Evolution strategies and genetic algorithms treat the model as a black box: they evaluate fitness based on outputs without requiring access to internal gradients. This property makes them applicable to non-differentiable objectives, discrete action spaces, hardware-in-the-loop optimization, and any setting where gradient computation is impractical. The trade-off is sample efficiency — black-box methods typically require more evaluations than gradient-based training.

### Population-Based Training

PBT maintains a population of models training concurrently and applies evolutionary pressure (selection, mutation) to hyperparameters during the training process. Unlike grid search or random search, PBT adapts hyperparameters as a function of training progress. This produces dynamic schedules (e.g., learning rate warm-up followed by decay) that emerge from selection pressure rather than manual design.

### Speciation

Speciation partitions a population into groups (species) of structurally similar individuals. Within each species, fitness is shared, preventing a single dominant topology from eliminating all others. This protects novel structures that may initially perform poorly but have long-term potential — analogous to ecological niches in biological evolution. NEAT uses genomic distance (based on disjoint and excess genes) to assign individuals to species.

## Getting Started

1. **Choose a framework.** For Python-based research, start with NEAT-Python for topology evolution or EvoJAX for large-scale ES experiments. For .NET integration, use SharpNEAT.

2. **Define the fitness function.** The fitness function is the single most important design decision. It determines what the evolutionary process optimizes. Start with a simple, well-understood objective before introducing multi-objective or novelty-based fitness.

3. **Start with small populations and simple problems.** Validate your setup on benchmark tasks (XOR for NEAT, CartPole for ES) before scaling to complex domains.

4. **Monitor diversity.** Track the number of species, the distribution of fitness across the population, and structural diversity. Premature convergence (the population collapsing to a single strategy) is the most common failure mode.

5. **Experiment with hyperparameters.** Mutation rates, crossover probability, speciation thresholds, and population size all interact. PBT can be applied meta-circularly to tune these evolutionary hyperparameters.

6. **Scale evaluation, not just population size.** The primary bottleneck in neuroevolution is evaluation cost. Invest in parallelism (GPU batching via EvoJAX, distributed evaluation) before increasing population size.

## Risks and Limitations

- **Sample inefficiency.** Neuroevolution typically requires orders of magnitude more evaluations than gradient-based methods for problems where gradients are available. Use it when gradients are unavailable, unreliable, or when the search space includes discrete structural decisions.

- **Fitness function misalignment.** Evolutionary processes optimize exactly what the fitness function measures. Poorly designed fitness functions lead to solutions that exploit loopholes rather than solving the intended problem. This is analogous to Goodhart's Law (see [reinforcement-learning.md](reinforcement-learning.md)).

- **Premature convergence.** Without sufficient diversity maintenance (speciation, novelty search, quality-diversity), populations collapse to local optima. Monitoring species count and behavioral diversity is essential.

- **Computational cost at scale.** Large populations of large models require substantial compute. Evolution strategies mitigate this somewhat through parallelism, but the total compute budget remains a practical constraint.

- **Reproducibility.** Stochastic search processes are inherently variable across runs. Report results over multiple seeds and characterize the distribution of outcomes, not just the best run.

- **Interpretability.** Evolved networks are often less interpretable than hand-designed architectures. Self-assembling and developmental approaches may exacerbate this, as the mapping from genotype to phenotype is indirect.

## See Also

- [papers.md](papers.md) — Consolidated bibliography with full citations
- [reinforcement-learning.md](reinforcement-learning.md) — Complementary optimization paradigm; ES and RL share problem domains
