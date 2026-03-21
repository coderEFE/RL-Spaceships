## Instructions for training
- Open Anaconda Powershell Prompt
- Run `conda activate mlagents` to activate mlagents python environment
- Navigate to RL-Spaceships directory on local machine with cd commands
- Run `mlagents-learn config/SpaceshipTwos.yaml --run-id=<YourRunID>`, replacing <YourRunID> with the unique name of this run
- To see Tensorboard results, run `tensorboard --logdir results --port 6006`

## Iterations of Configurations and Reward Structures for 2v2 Ships
- `TwoVsTwo`:
    - Penalty for touching walls
    - +1 to team when resource collected
    - no friendly fire
    - -3 penalty to team when all agents on team are killed
    - Smaller (20 or 25 length?) raycast perception

    OUTCOME: Ships navigated to asteroids and shot them to collect resources. Since there was no penalty for shooting, they would spam the laser every second, but it seemed like they would intentional aim toward enemy ships to kill them if they were nearby. They prioritized shooting toward asteroids.

- `TwoVsTwoFriendlyFire`:
    - Penalty for touching walls
    - +1 to team when resource collected, friendly fire
    - -2 penalty to team when any agents on team is killed
    - Longer (40 length) raycast perception

    OUTCOME: Ships behaved similarly to TwoVsTwo, except that their movement was more stuttered, as if they were hesitant to move because they could get killed more easily

- `TwoVsTwo2`:
    - Penalty for touching walls
    - +1 to team when resource collected
    - no friendly fire
    - Longer (40 length) raycast perception
    - -0.1f when firing laser
    - added another observation for whether teammate is alive or not (true or false) - this brings vector observation space size up to 5
    - Ships don't drop resources

    OUTCOME: No jitter/stutter in movement at all compared to TwoVsTwoFriendlyFire and TwoVsTwo (which only had a little). Ships had a strategy of racing as fast as they can to asteroids and only shooting once they were next to the asteroid, immediately collecting the resource. This meant that there was less risk of shooting other ships. Ships did not shoot at each other and hardly ever died, which also might have been because there was no penalty for dying (although I thought that would realize that they could not collect as many resources if they died, and that they could collect more if they killed other ships). It would be interesting to look through history of training to see if ships did shoot each other in past, but realized peace was more beneficial.

- TODO: `TwoVsTwoNoShipResources`:
    - Penalty for touching walls
    - +1 to team when resource collected
    - no friendly fire
    - Longer (40 length) raycast perception
    - -0.1f when firing laser
    - observation for teammate is alive or not (true or false)
    - Clustered random spawning for both teams, instead of purely random
    - Ships don't drop resources

- `TwoVsTwo3`:
    - Penalty for touching walls
    - +1 to team when resource collected
    - no friendly fire
    - Longer (40 length) raycast perception
    - -0.1f when firing laser
    - observation for whether teammate is alive or not (true or false) - this brings vector observation space size up to 5
    - Clustered random spawning for both teams, instead of purely random
    - Ships drop a single resource when killed

    OUTCOME: There was more stop-and-go/stuttering movements than in TwoVsTwo2. Ships would often shoot each other if they were in the flight path of others, sometimes going a little out of their way to attack them, although mostly gravitating toward asteroids. Ships were slower at getting to asteroids than in TwoVsTwo2 and sometimes went in circles for a few seconds if there was a single asteroid left, although this was a rare behavior. I logged custom stats over the training and saw that the # of ships alive trended downward as training progressed. Because of small penalty for shooting, ships were more deliberate with their shots.

- `TwoVsTwo4`:
    - Penalty for touching walls
    - +1 to team when resource collected
    - no friendly fire
    - Longer (40 length) raycast perception
    - -0.1f when firing laser
    - observation for whether teammate is alive or not (true or false) - this brings vector observation space size up to 5
    - Clustered random spawning for both teams, instead of purely random
    - Ships drop a single resource when killed
    - -1 penalty to teams when an agent dies

    OUTCOME: I expected that ships would be less likely to fight each other because of the added penalty, but the outcome and metrics look very similar to TwoVsTwo3. Ships often go out of their way to fight. Also, I observe that when faced with an asteroid and an exposed resource, the ships often go for the exposed resource even if they are further away, then they mine the asteroid after. Not always, though.

- `TwoVsTwo5`:
    - Penalty for touching walls
    - +1 to team when resource collected
    - no friendly fire
    - Longer (40 length) raycast perception
    - -0.1f when firing laser
    - observation for whether teammate is alive or not (true or false) - this brings vector observation space size up to 5
    - Clustered random spawning for both teams, instead of purely random
    - Ships drop a single resource when killed
    - -2 penalty to teams when an agent dies

    OUTCOME: Behavior was still very similar to TwoVsTwo4 and TwoVsTwo3, even with increased penalty for dying. The group cumulative reward is a bit lower overall, but it's still higher than in TwoVsTwo2 (where agents did not drop resources), so I hypothesize that the agents will have less conflict when penalty is even greater.

- `TwoVsTwo6`:
    - Penalty for touching walls
    - +1 to team when resource collected
    - no friendly fire
    - Longer (40 length) raycast perception
    - -0.1f when firing laser
    - observation for whether teammate is alive or not (true or false) - this brings vector observation space size up to 5
    - Clustered random spawning for both teams, instead of purely random
    - Ships drop a single resource when killed
    - -4 penalty to teams when an agent dies

    OUTCOME: Ships still went after each other pretty regularly, so overall behavior is similar to previous configurations, even with the higher penalty. In the tensorboard graphs, the GroupCumulativeReward is lower than that of TwoVsTwo2, so it would have been better for ships to just go after asteroids and not kill each other, but they didn't figure that out. I think one reason is that ships discover early on that they can get a reward by killing other ships (as long as they don't die as well), which is hard to unlearn later. The NumTotalAlive is slightly higher on average. Might need to tweak hyperparameters or train for longer to see if they realize this.

- `TwoVsTwo7`:
    - Penalty for touching walls
    - +1 to team when resource collected
    - no friendly fire
    - Longer (40 length) raycast perception
    - -0.1f when firing laser
    - observation for whether teammate is alive or not (true or false) - this brings vector observation space size up to 5
    - Clustered random spawning for both teams, instead of purely random
    - Ships drop a single resource when killed
    - -8 penalty to teams when an agent dies

    OUTCOME: Despite having a much lower GroupCumulativeReward than previous configurations, ships still shoot each other regularly. NumTotalAlive is quite higher than previous configurations, but is still decreasing over time and overall behavior seems the same. Not sure why, but some of the tensorboard graphs stop showing data after a point in training. Ships did not learn to optimize their shooting much (probably because of the magnitude of the penalties they are recieving) and shoot about every second. Ships are very hesitant to move and pause regularly.

- `RewardingAsteroids`:
    - Penalty for touching walls
    - +1 to team when resource collected
    - no friendly fire
    - Longer (40 length) raycast perception
    - -0.1f when firing laser
    - observation for whether teammate is alive or not (true or false) - this brings vector observation space size up to 5
    - Clustered random spawning for both teams, instead of purely random
    - Ships drop a single resource when killed
    - -2 penalty to teams when an agent dies
    - Asteroids drop 2 resources

    OUTCOME: According to the NumTotalAlive graph, more ships were alive at end of episode on average when asteroids dropped 2 resources compared to 1. However, the number of total alive was still decreasing over time and the behavior of agents appeared to still be killing each other.

- `Symbiotic`:
    - 3 orange asteroids and 3 blue asteroids. Blue ships can only destroy blue asteroids and collect blue resources. Orange ships can only destroy orange asteroids and collect orange resources. However, asteroids of a color drop resources of the opposite color.
    - Blue ships drop orange resource and orange ships drop blue resource when killed
    - Similar config to RewardingAsteroids except for the following notable configs:
    - -1 penalty to teams when agent dies
    - -0.1f when firing laser
    - Asteroids drop 1 resource

    OUTCOME: Ships just kill each other and don't attack asteroids because of the short-term rewards they get. They don't realize they could collaborate for higher overall resources/rewards.

- `Symbiotic2`:
    - 3 orange asteroids and 3 blue asteroids. Blue ships can only destroy blue asteroids and collect blue resources. Orange ships can only destroy orange asteroids and collect orange resources. However, asteroids of a color drop resources of the opposite color.
    - Ships don't drop any resources
    - Similar config to RewardingAsteroids except for the following notable configs:
    - -1 penalty to teams when agent dies
    - -0.1f when firing laser
    - Asteroids drop 1 resource

    OUTCOME: Ships just fly around randomly and don't attack each other or asteroids

- `Symbiotic3`:
    - 3 orange asteroids and 3 blue asteroids. Blue ships can only destroy blue asteroids and collect blue resources. Orange ships can only destroy orange asteroids and collect orange resources. However, asteroids of a color drop resources of the opposite color.
    - Ships don't drop any resources
    - Similar config to RewardingAsteroids except for the following notable configs:
    - -1 penalty to teams when agent dies
    - No penalty for firing laser
    - Asteroids drop 1 resource
    - Stopped training at about 1 million steps because strategy was not improving

    OUTCOME: Ships don't seem to have a clear strategy or learn to destroy all asteroids consistently. However, there are some cases where ships find a strategy of moving in circles around an asteroid of the opposite color and pushing it around the environment, likely because that asteroid will drop a resource they can collect if destroyed. They might be trying to push it closer to other team so it will be destroyed.

- `Symbiotic4`:
    - 0.2f reward for shooting asteroids
    - 1 million steps

- `Symbiotic5`:
    - 0.2f reward for shooting asteroids
    - cut off a little before 1 million steps
    - Intsead of -1 group penalty for agents dying, it is changed to an individual penalty

    OUTCOME: NumTotalAlive is higher than in Symbiotic4, likely because the -1 individal penalty makes ships understand how to avoid death better. Number of asteroids mined is slightly more than Symbiotic4. Ships shoot whenever they can and are very trigger happy, which still ends up killing other ships sometimes.

- `Symbiotic5-1`:
    - 0.2f reward for shooting asteroids
    - --initialize-from=Symbiotic5
    - Add 0.1 penalty for firing laser
    - Another 1 million steps

    OUTCOME: NumTotalAlive is much higher than than in Symbiotic5. Ships are more intentional with their shooting. Number of asteroids mined is slightly more than Symbiotic5. Just like previous configs, ships often seem to get confused and go in circles for a while before mining an asteroid, but are quicker to move towards resources. Might be because the 0.2f incentive to mine asteroids is pretty weak.

- `RewardingAsteroids2`:
    - -2 individual penalty for dying
    - Laser length reduced to 5 units

- `RewardingAsteroids3`:
    - -4 individual penalty for dying
    - Laser length reduced to 5 units

- `RewardingAsteroids4`:
    - -8 individual penalty for dying
    - Laser length reduced to 5 units

- `Symbiotic6`:
    - No reward for shooting asteroids
    - No penalty for firing laser
    - No penalty for dying
    - Swap out observation of teammate being alive or not with an observation of the normalized value of how many agents on other team are alive (1 being all of them, 0.5 being half of them, 0 being none of them)
    - 2 million steps
    - Use SpaceshipTwosLSTM.yaml config file which has LSTM with sequence_length: 128 and memory_size: 128.

    - TODO: could train this one for longer to see if interesting behaviors converge

- `TwoVsTwoLSTM`:
    - TODO: Try LSTM with TwoVsTwo setup where agents do not drop resources. Use shorter lasers. Has observation of enemy team being alive. See if agents fight each other when they realize that killing other team leads to more chances for them to get resources.

Potential new setup for draft of honors research: Just manipulative penalty magnitude for dying, and manipulate amount of resources dropped by asteroids. Has 6 vector observations because it includes proportion of this team agents alive and proportion of other team agents alive. Have 6 asteroids for a "perfect nash equilibrium" so its not inherently competitive. Could make alternate asteroids drop a different kind of resource with a higher reward. I should probably stick with regular beta (0.005) because increasing beta didn't seem to lead to new strategies
Try with 10 conditions (2 types of rewards that asteroids drop with 5 types of penalties)

- `Beta1RewardAsteroids1Penalty`:
    - -1 group penalty for dying
    - 5 unit laser length
    - randomized cluster spawning
    - 6 asteroids
    - 6 vector observations
    - 0.01 beta (doubled). 
    - Train for 4 million steps

- `1RewardAsteroids0Penalty`:
    - No group penalty for dying
    - 5 unit laser length
    - randomized cluster spawning
    - 6 asteroids
    - 6 vector observations
    - Train for 2 million steps

- `1RewardAsteroids1Penalty`:
    - -1 group penalty for dying
    - 5 unit laser length
    - randomized cluster spawning
    - 6 asteroids
    - 6 vector observations
    - Train for 2 million steps

- `1RewardAsteroids2Penalty`:
    - -2 group penalty for dying
    - 5 unit laser length
    - randomized cluster spawning
    - 6 asteroids
    - 6 vector observations
    - Train for 2 million steps

- `1RewardAsteroids4Penalty`:
    - -4 group penalty for dying
    - 5 unit laser length
    - randomized cluster spawning
    - 6 asteroids
    - 6 vector observations
    - Train for 2 million steps

- `1RewardAsteroids8Penalty`:
    - -8 group penalty for dying
    - 5 unit laser length
    - randomized cluster spawning
    - 6 asteroids
    - 6 vector observations
    - Train for 2 million steps

### General Observations
- While the number of total agents alive tends to decrease and flatten out over training steps, I observe that there are oscillating peaks and valleys in the graphs for number of blue or orange agents alive, and that these oscillations are opposite to those of the other graph at same training step. The average distance between each oscillation is about 200,000 steps, which I think is because the self_play team_change value in configuration.yaml is 200000.

### Ideas
- Could have ships all start on opposite corners of one side and have asteroids spawn on the other side, so that ships can decide to split up or kill each other at beginning, rather than always going for asteroid closest to them.
- Could have ships drop a resource when killed. That way ships are incentivized for killing other ships, yet ships only drop 1 resource even if they collected > 1.
- Could some kind of tragedy of the commons aspect or something that requires collaboration between teams instead of greed, such as in the altrustic/egoistic/opportunistic paper
- Some interesting MARL game design ideas for emergent behavior: https://gemini.google.com/app/b922f5d1a6cbb2b0
- Could take a pretrained network that used random agent and asteroid positions and see if it generalizes to situation where agent team spawn together on opposite sides
- Try out checkpoint models saved in the middle of training the TwoVsTwo2 game to see if agents sought conflict more before realizing cooperation was better. Would be good to log graphs during training.
- Clustered random spawning, where I spawn blue team agents within a small radius of a random center point, then pick a random point for orange team agents that is a minimum distance away from blue team and small them within small radius of that point (so that teams spawn together but prevent overfitting by having them spawn same spot every time).
- Should try increasing the penalty that teams take when agents die to see if it reduces conflict (like -2 and -3)
- Should try increasing the reward of resources dropped by asteroids relative to those by agents to see if it reduces conflict
- Could track metric of when ships die in the episode, to see if ships are fighting more earlier on in different configurations
- Could try setup recommended by Gemini with 2 different types of asteroids and resources so that one team can only pickup resources from asteroids that only the other team can shoot. This would lead to some interesting collaboration. Could also have agents drop the resource that other team can pick up when they are killed, to incentize some conflict, but then agents realize that they need other team to mine asteroids for them.
- Could also try training for a while with situation like TwoVsTwo2 and then add in where agents drop resources when killed (using train from previous model)
- Could give agents normalized position of their teammate for more informed collaboration
- Could try to stack last 2 frames for ray perception sensor to give agent knowledge of the movement of their surroundings, but maybe that would take longer for a strategy to converge because of added complexity
- Consider increasing time_horizon hyperparameter so that agents will remember that killing other agents leads to less long-term rewards
- Could give agents an observation on whether other team is dead or alive (boolean)
- Could increase beta hyperparameter to 0.01 (instead of 0.005) to match other POCA configs and encourage more exploration if entropy is decreasing too quickly
- Could add memory hyperparameters in config file to give agents an LSTM like Hallway environment. Great explanation here: https://discussions.unity.com/t/lstm-unity-ml-agents/869919
- Try extending LSTM sequence_length and see if it hits a max
- Could add a "stag"/big asteroid that requires both teams to destroy it and rewards both teams when destroyed
- Could have asteroids that respawn so that having more agents alive would help them all get higher rewards because more asteroid would be mined at once
- Could also try a setup where only one team can shoot lasers and only one team can pick up resources, but that picking up resources rewards all teams. That way the team that shoots lasers is rewarded in the future for their actions.
- Could try having one team use a pretrained model that only goes after asteroids (was trained when agents did not drop resources) and another team use a model like 1RewardAsteroids1Penalty. Could see if the agents adjust their strategies at all
- Try a setup where there are no small asteroids, just one large asteroid in the middle that requires agents from both team to touch it to give out rewards to both teams. Agents still drop resources when killed.