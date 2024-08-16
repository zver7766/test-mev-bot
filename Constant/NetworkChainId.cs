using System.ComponentModel;

namespace DexCexMevBot.Constant;

public enum NetworkChainId
{
    eth = 1,
    bsc = 56,
    heco = 128,
    ftm = 250,
    ava = 43114,
    pol = 137,
    [Description("bsc-tst")]
    bsctst = 97,
    ropsten = 3,
    harmony = 1666600000,
    optimism = 10,
    arbitrum = 42161,
    goerli = 5,
    sol = 9999
}