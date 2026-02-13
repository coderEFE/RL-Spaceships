## Instructions for training
- Open Anaconda Powershell Prompt
- Run `conda activate mlagents` to activate mlagents python environment
- Navigate to RL-Spaceships directory on local machine with cd commands
- Run `mlagents-learn config/SpaceshipTwos.yaml --run-id=<YourRunID>`, replacing <YourRunID> with the unique name of this run

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

    OUTCOME: No jitter/stutter in movement at all compared to TwoVsTwoFriendlyFire and TwoVsTwo (which only had a little). Ships had a strategy of racing as fast as they can to asteroids and only shooting once they were next to the asteroid, immediately collecting the resource. This meant that there was less risk of shooting other ships. Ships did not shoot at each other and hardly ever died, which also might have been because there was no penalty for dying (although I thought that would realize that they could not collect as many resources if they died, and that they could collect more if they killed other ships). It would be interesting to look through history of training to see if ships did shoot each other in past, but realized peace was more beneficial.

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

### Ideas
- Could have ships all start on opposite corners of one side and have asteroids spawn on the other side, so that ships can decide to split up or kill each other at beginning, rather than always going for asteroid closest to them.
- Could have ships drop a resource when killed. That way ships are incentivized for killing other ships, yet ships only drop 1 resource even if they collected > 1.
- Could some kind of tragedy of the commons aspect or something that requires collaboration between teams instead of greed, such as in the altrustic/egoistic/opportunistic paper
- Some interesting MARL game design ideas for emergent behavior: https://gemini.google.com/app/b922f5d1a6cbb2b0
- Could take a pretrained network that used random agent and asteroid positions and see if it generalizes to situation where agent team spawn together on opposite sides
- Try out checkpoint models saved in the middle of training the TwoVsTwo2 game to see if agents sought conflict more before realizing cooperation was better. Would be good to log graphs during training.
- Clustered random spawning, where I spawn blue team agents within a small radius of a random center point, then pick a random point for orange team agents that is a minimum distance away from blue team and small them within small radius of that point (so that teams spawn together but prevent overfitting by having them spawn same spot every time).