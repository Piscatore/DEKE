# Papers

A consolidated bibliography of papers referenced across the science documentation, organized by topic. Each entry includes the title, authors or institution, year and venue, a brief summary, and a link where available.

## Evolution and Neuroevolution

**Stanley, K. O. & Miikkulainen, R. (2002).** "Evolving Neural Networks through Augmenting Topologies." *Evolutionary Computation*, 10(2), 99--127.
Introduced NEAT: historical markings for crossover across different topologies, speciation to protect structural innovation, and complexification from minimal networks. The foundational algorithm for topology-evolving neuroevolution.
[https://nn.cs.utexas.edu/downloads/papers/stanley.ec02.pdf](https://nn.cs.utexas.edu/downloads/papers/stanley.ec02.pdf)

**Real, E., Liang, C., So, D. R., & Le, Q. V. (2020).** "AutoML-Zero: Evolving Machine Learning Algorithms From Scratch." *ICML 2020*.
Evolved complete learning algorithms from basic math operations, rediscovering backpropagation and learning rate schedules without human-designed components in the search space.
[https://arxiv.org/abs/2003.03384](https://arxiv.org/abs/2003.03384)

**Salimans, T., Ho, J., Chen, X., Sidor, S., & Sutskever, I. (2017).** "Evolution Strategies as a Scalable Alternative to Reinforcement Learning." *arXiv:1703.03864*.
Demonstrated that Gaussian-perturbation evolution strategies solve RL benchmarks (Atari, MuJoCo) competitively with policy gradient methods, with embarrassingly parallel evaluation and no backpropagation requirement.
[https://arxiv.org/abs/1703.03864](https://arxiv.org/abs/1703.03864)

**Jaderberg, M., Dalibard, V., Osindero, S., et al. (2017).** "Population Based Training of Neural Networks." *arXiv:1711.09846*.
Combined random hyperparameter search with online model selection: underperformers copy weights from better models and perturb hyperparameters, producing adaptive schedules that emerge from selection pressure.
[https://arxiv.org/abs/1711.09846](https://arxiv.org/abs/1711.09846)

**Green, C. (2024).** *SharpNEAT*. Open-source .NET implementation of NEAT and HyperNEAT.
High-performance NEAT implementation for .NET with configurable activation functions, speciation strategies, and large population support.
[https://github.com/colgreen/sharpneat](https://github.com/colgreen/sharpneat)

**Tang, Y., Tian, Y., Ha, D., et al. (2022).** "EvoJAX: Hardware-Accelerated Neuroevolution." *arXiv:2202.05008*.
JAX-based neuroevolution library leveraging GPU/TPU parallelism for large-population evolution strategies and genetic algorithm experiments.
[https://arxiv.org/abs/2202.05008](https://arxiv.org/abs/2202.05008)

**Plantec, E., Hamon, G., Etcheverry, M., Oudeyer, P.-Y., Moulin-Frier, C., & Chan, B. W.-C. (2024).** "Growing Artificial Neural Networks with Self-Assembling Processes." *ALIFE 2024*.
Applied developmental (morphogenetic) growth rules to construct neural networks, evolving the developmental program rather than the network directly. Produced modular architectures robust to damage.

**arXiv:2509.24372 (2025).** "Evolution Strategies at Scale."
Extended the OpenAI 2017 ES work to modern distributed settings with variance reduction, structured exploration, and communication-efficient implementations across thousands of workers.
[https://arxiv.org/abs/2509.24372](https://arxiv.org/abs/2509.24372)

## Reinforcement Learning and Feedback

**Schultz, W., Dayan, P., & Montague, P. R. (1997).** "A Neural Substrate of Prediction and Reward." *Science*, 275(5306), 1593--1599.
Established that phasic dopamine neuron firing encodes temporal difference error, linking computational RL to neuroscience.

**Sutton, R. S. & Barto, A. G. (2018).** *Reinforcement Learning: An Introduction* (2nd ed.). MIT Press.
Comprehensive textbook covering temporal difference learning, eligibility traces, policy gradient methods, and function approximation. The standard reference for the field.
[http://incompleteideas.net/book/the-book.html](http://incompleteideas.net/book/the-book.html)

**Pathak, D., Agrawal, P., Efros, A. A., & Darrell, T. (2017).** "Curiosity-driven Exploration by Self-Supervised Prediction." *ICML 2017*.
Introduced the Intrinsic Curiosity Module (ICM): a forward model in learned feature space whose prediction error serves as intrinsic reward, driving exploration in sparse-reward environments.
[https://arxiv.org/abs/1705.05363](https://arxiv.org/abs/1705.05363)

**Burda, Y., Edwards, H., Storkey, A., & Klimov, O. (2018).** "Exploration by Random Network Distillation." *arXiv:1810.12894*.
Addressed the noisy TV problem in curiosity-driven exploration by using a fixed random network as the prediction target, making the curiosity signal sensitive to novelty rather than stochasticity.
[https://arxiv.org/abs/1810.12894](https://arxiv.org/abs/1810.12894)

**Andrychowicz, M., Wolski, F., Ray, A., et al. (2017).** "Hindsight Experience Replay." *NeurIPS 2017*.
Introduced hindsight relabeling: failed trajectories are replayed with the achieved state as the goal, converting failures into successful training examples for goal-conditioned RL.
[https://arxiv.org/abs/1707.01495](https://arxiv.org/abs/1707.01495)

**Gao, L., Schulman, J., & Hilton, J. (2022).** "Scaling Laws for Reward Model Overoptimization." *arXiv:2210.10760*.
Empirically characterized Goodhart's Law in RLHF: as KL divergence from the reference policy increases, proxy reward rises while true reward degrades. Quantified the over-optimization boundary.
[https://arxiv.org/abs/2210.10760](https://arxiv.org/abs/2210.10760)

## Quality-Diversity

**Mouret, J.-B. & Clune, J. (2015).** "Illuminating Search Spaces by Mapping Elites." *arXiv:1504.04909*.
Introduced MAP-Elites, a quality-diversity algorithm that maintains a grid of the best solutions across a user-defined behavior space, simultaneously optimizing performance and behavioral diversity.
[https://arxiv.org/abs/1504.04909](https://arxiv.org/abs/1504.04909)

## Prompt Optimization

**arXiv:2507.19457 (2025).** "GEPA: Guided Evolution for Prompt Adaptation."
Applies evolutionary search to prompt optimization, mutating and recombining prompts using fitness signals from task performance. Demonstrates that evolved prompts can outperform hand-crafted ones on diverse benchmarks.
[https://arxiv.org/abs/2507.19457](https://arxiv.org/abs/2507.19457)

**EvoAgentX.** Open-source framework for evolutionary prompt and agent optimization.
Provides tools for evolving agent configurations, prompts, and tool-use strategies through population-based methods. Integrates with standard LLM APIs.
[https://github.com/EvoAgentX/EvoAgentX](https://github.com/EvoAgentX/EvoAgentX)

## Self-Evolving AI Agents

**arXiv:2508.07407 (2025).** "A Survey on Self-Evolving AI Agents." University of Glasgow / University of Sheffield.
Comprehensive survey of AI agents that modify their own behavior, architecture, or objectives over time. Covers self-play, meta-learning, evolutionary self-modification, and open-ended learning. Identifies key challenges including stability, safety, and evaluation of self-modifying systems.
[https://arxiv.org/abs/2508.07407](https://arxiv.org/abs/2508.07407)

## AI Safety and Philosophy

**Bozkurt, A. (2025).** "Three Laws of AI."
Proposes normative principles for AI system design inspired by Asimov's Laws of Robotics but grounded in contemporary AI capabilities. Addresses value alignment, corrigibility, and the tension between autonomy and human oversight.

**Elish, M. C. (2019).** "Moral Crumple Zones: Cautionary Tales in Human-Robot Interaction." *Engaging Science, Technology, and Society*, 5, 40--60.
Introduced the concept of the "moral crumple zone" — the phenomenon where human operators absorb blame for failures in automated systems they cannot fully control. Relevant to the design of autonomous agents that share decision-making with humans.

## Retrieval and RAG

**Liu, N. F., Lin, K., Hewitt, J., Paranjape, A., Bevilacqua, M., Petroni, F., & Liang, P. (2023).** "Lost in the Middle: How Language Models Use Long Contexts." *arXiv:2307.03172*.
Demonstrated that LLMs underutilize information placed in the middle of the context window, with best performance for information at the beginning or end. Directly informs context assembly strategies in RAG pipelines.
[https://arxiv.org/abs/2307.03172](https://arxiv.org/abs/2307.03172)

**Gao, L., Ma, X., Lin, J., & Callan, J. (2022).** "Precise Zero-Shot Dense Retrieval without Relevance Labels." *arXiv:2212.10496*.
Introduced HyDE (Hypothetical Document Embeddings): generate a hypothetical answer with an LLM, embed it, and use it as the retrieval query. Improves zero-shot dense retrieval by bridging the gap between short queries and long documents.
[https://arxiv.org/abs/2212.10496](https://arxiv.org/abs/2212.10496)

**Zheng, H. S., Mishra, S., Chen, X., Cheng, H.-T., Chi, E. H., Le, Q. V., & Zhou, D. (2023).** "Take a Step Back: Evoking Reasoning via Abstraction in Large Language Models." *arXiv:2310.06117*.
Introduced step-back prompting: generating a more abstract version of the query before retrieval or reasoning, improving performance on knowledge-intensive tasks.
[https://arxiv.org/abs/2310.06117](https://arxiv.org/abs/2310.06117)

**Robertson, S. & Zaragoza, H. (2009).** "The Probabilistic Relevance Framework: BM25 and Beyond." *Foundations and Trends in Information Retrieval*, 3(4), 333--389.
Comprehensive treatment of the BM25 family of retrieval functions, covering the probabilistic foundations, parameter tuning, and extensions. The standard reference for sparse retrieval.

**RAGAS Framework.** Retrieval-Augmented Generation Assessment.
Open-source evaluation framework for RAG pipelines providing metrics for faithfulness, answer relevance, context precision, and context recall. Used as a standard benchmark for RAG quality.
[https://github.com/explodinggradients/ragas](https://github.com/explodinggradients/ragas)

## See Also

- [neuroevolution.md](neuroevolution.md) — Detailed discussion of neuroevolution concepts and frameworks
- [reinforcement-learning.md](reinforcement-learning.md) — TD learning, curiosity, RLHF failure modes, and quality-diversity
- [retrieval-theory.md](retrieval-theory.md) — Chunking, hybrid search, re-ranking, and evaluation metrics
