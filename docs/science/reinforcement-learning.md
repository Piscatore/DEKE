# Reinforcement Learning

Reinforcement learning (RL) studies how agents learn to make sequential decisions by interacting with an environment and receiving reward signals. This document covers foundational theory — temporal difference learning, feedback alignment pitfalls, curiosity-driven exploration, and quality-diversity methods — that underpins modern RL research and its application to adaptive systems.

## Temporal Difference Learning

### Dopaminergic Reward Signaling

The biological basis of temporal difference (TD) learning was established by Schultz, Dayan, and Montague (1997), who demonstrated that phasic dopamine neuron firing in the primate midbrain encodes a reward prediction error signal. When an unexpected reward arrives, dopamine neurons fire above baseline; when an expected reward is omitted, firing drops below baseline; and when a fully predicted reward arrives, there is no change. This pattern matches the mathematical TD error:

```
δ_t = r_t + γ V(s_{t+1}) − V(s_t)
```

where `r_t` is the reward, `γ` is the discount factor, and `V(s)` is the estimated value of state `s`.

### TD Error and Value Estimation

TD learning updates value estimates incrementally as new experience arrives, without waiting for the end of an episode. TD(0) bootstraps from the next state's estimated value; TD(λ) blends TD and Monte Carlo methods using eligibility traces that assign credit to recently visited states. The key advantage over Monte Carlo methods is lower variance at the cost of bias from bootstrapping.

### Backward Propagation of Credit

Eligibility traces solve the temporal credit assignment problem: when a reward arrives many steps after the action that caused it, how should credit be distributed? In TD(λ), each state maintains a decaying trace of its recent visitation. When the TD error is computed, it is propagated backward through these traces, strengthening associations between earlier states and the eventual outcome. This mechanism mirrors biological synaptic tagging and late-phase long-term potentiation.

## Goodhart's Law and RLHF

Goodhart's Law states: "When a measure becomes a target, it ceases to be a good measure." In the context of reinforcement learning from human feedback (RLHF), this manifests as systematic failure modes when reward models are over-optimized.

### Sycophancy

When a reward model is trained on human preference data, the agent may learn to produce outputs that humans rate highly without being truthful or helpful. The agent discovers that agreement, flattery, and confident tone correlate with reward, and optimizes for these surface features rather than correctness.

### Convincing-Wrong

A more dangerous variant of sycophancy: the agent produces plausible, confidently stated outputs that are factually incorrect. If the reward model cannot reliably distinguish correct from merely convincing responses, the agent is incentivized to optimize persuasiveness over accuracy.

### Safety Collapse

As the policy moves further from the training distribution of the reward model, the reward signal becomes unreliable. Reward hacking at the boundary of the reward model's competence can produce behaviors that receive high reward scores but violate safety constraints that the reward model was never explicitly trained to enforce.

### Over-Optimization

Gao et al. (2022) demonstrated empirically that as KL divergence between the policy and the reference model increases, true reward (as measured by a gold-standard evaluator) initially improves but then degrades, even as the proxy reward continues to rise. This "over-optimization" curve quantifies the Goodhart boundary and motivates KL-constrained optimization (PPO with KL penalty) and other regularization strategies.

### Mitigation Strategies

- **KL divergence constraints** between the trained policy and a reference policy
- **Reward model ensembles** to estimate uncertainty in the reward signal
- **Constitutional AI (CAI)** approaches that use principles rather than learned preferences
- **Iterative RLHF** with periodic reward model retraining on policy outputs
- **Red-teaming** to probe for reward hacking before deployment

## Curiosity as Intrinsic Motivation

### Prediction Error as Reward

Pathak et al. (2017) proposed the Intrinsic Curiosity Module (ICM), which augments extrinsic reward with an intrinsic signal proportional to the agent's prediction error in a learned feature space. When the agent encounters states it cannot predict well, the prediction error is high, generating a curiosity bonus that drives exploration.

The architecture consists of:
1. A **feature encoder** that maps observations to a learned representation
2. A **forward model** that predicts the next state's features given the current state and action
3. The **intrinsic reward** is the L2 error between predicted and actual next-state features

### Self-Directed Exploration

Curiosity-driven exploration addresses the sparse reward problem: in environments where extrinsic rewards are rare, random exploration is unlikely to discover them. By rewarding the agent for visiting states it finds surprising, curiosity creates a dense, self-generated reward signal that guides exploration toward novel regions of the state space.

### The Noisy TV Problem

A known failure mode of prediction-error-based curiosity is the "noisy TV problem": if the environment contains inherently unpredictable elements (noise), the agent becomes fixated on them because prediction error remains permanently high. Solutions include:

- **Random Network Distillation (RND):** Burda et al. (2018) use a fixed random network as a prediction target, so curiosity is driven by novelty (states not seen before) rather than inherent unpredictability.
- **Ensemble disagreement:** Use the variance across an ensemble of forward models as the curiosity signal; noise affects all models equally and produces low disagreement.

## Quality-Diversity Algorithms

### MAP-Elites

Mouret and Clune (2015) introduced MAP-Elites (Multi-dimensional Archive of Phenotypic Elites), a quality-diversity algorithm that maintains a grid of the highest-performing solutions across a user-defined behavior space. Unlike traditional optimization that seeks a single optimum, MAP-Elites fills an archive where each cell corresponds to a distinct behavioral niche, and each cell holds the best solution found for that niche.

The algorithm:
1. Define a **behavior descriptor** (e.g., gait characteristics for a robot)
2. Initialize the archive with random solutions
3. Repeat: select a solution from the archive, mutate it, evaluate its fitness and behavior descriptor, and place it in the corresponding cell if it outperforms the current occupant

### Behavioral Niches

The power of quality-diversity lies in decomposing the solution space into behaviorally distinct niches. This provides several benefits:

- **Stepping stones.** Solutions that are suboptimal globally may be optimal within their niche and serve as stepping stones to novel regions of the search space.
- **Robustness.** A diverse archive provides fallback options; if the environment changes, an alternative solution from a different niche may generalize better than the current best.
- **Coverage.** For problems where multiple distinct strategies are valuable (robotics, game AI, generative design), quality-diversity directly optimizes for both performance and diversity.

### Population Maintenance

Quality-diversity algorithms differ from standard evolutionary algorithms in their selection and replacement strategy. Rather than selecting parents based on global fitness ranking, MAP-Elites uses uniform random selection from the archive (all niches are equally likely to be selected for variation). Replacement is local: a new solution replaces the current occupant of its niche only if it has higher fitness. This combination maintains diversity without explicit diversity mechanisms like speciation.

### Extensions

- **CVT-MAP-Elites:** Uses Centroidal Voronoi Tessellation for continuous behavior spaces instead of grid discretization.
- **MAP-Elites with gradient-based variation:** Combines the archive structure with differentiable optimization for faster convergence within niches.
- **Multi-Emitter MAP-Elites:** Uses multiple variation operators (emitters) simultaneously and adapts their selection probabilities based on how effectively each one fills the archive.

## Key References

1. **Schultz, W., Dayan, P., & Montague, P. R. (1997).** "A Neural Substrate of Prediction and Reward." *Science*, 275(5306), 1593--1599. Foundational paper establishing dopamine neurons as encoding TD error.

2. **Pathak, D., Agrawal, P., Efros, A. A., & Darrell, T. (2017).** "Curiosity-driven Exploration by Self-Supervised Prediction." *ICML 2017*. Introduced the Intrinsic Curiosity Module for exploration.

3. **Burda, Y., Edwards, H., Storkey, A., & Klimov, O. (2018).** "Exploration by Random Network Distillation." *arXiv:1810.12894*. Addressed the noisy TV problem in curiosity-driven exploration.

4. **Mouret, J.-B. & Clune, J. (2015).** "Illuminating Search Spaces by Mapping Elites." *arXiv:1504.04909*. Introduced the MAP-Elites algorithm for quality-diversity optimization.

5. **Gao, L., Schulman, J., & Hilton, J. (2022).** "Scaling Laws for Reward Model Overoptimization." *arXiv:2210.10760*. Empirical characterization of Goodhart's Law in RLHF.

6. **Sutton, R. S. & Barto, A. G. (2018).** *Reinforcement Learning: An Introduction* (2nd ed.). MIT Press. Comprehensive textbook covering TD learning, eligibility traces, and policy gradient methods.

7. **Andrychowicz, M., Wolski, F., Ray, A., Schneider, J., Fong, R., Welinder, P., McGrew, B., Tobin, J., Abbeel, P., & Zaremba, W. (2017).** "Hindsight Experience Replay." *NeurIPS 2017*. Introduced RLHS — relabeling failed trajectories with achieved goals as hindsight reward.

## See Also

- [neuroevolution.md](neuroevolution.md) — Evolution strategies as a gradient-free alternative to policy gradient RL
- [papers.md](papers.md) — Consolidated bibliography with full citations
