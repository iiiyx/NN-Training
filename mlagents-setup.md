1. install nvidia cuda, cudnn, tensorrt - D:\Temp
2. install miniconda - D:\Temp
3. conda create -n tf python=3.8
4. conda activate
5. pip install --upgrade pip
6. pip install mlagents
7. edit mlagents: https://github.com/Unity-Technologies/ml-agents/issues/5811
8. pip install torch torchvision torchaudio
9. mlagents-learn Assets/Config/tankai_config.yaml --run-id=TankAI --torch-device cuda --resume