
# CoinMixer

A Bitcoin mixer is one way to maintain your privacy on the Bitcoin network.  Here’s how one popular mixer https://coinmixer.se/en/ works:

You provide  a list of new, unused addresses that you own to the mixer;
The mixer provides you with a new deposit address that it owns;
You transfer your bitcoins to that address;
The mixer will detect your transfer by watching or polling the P2P Bitcoin network;
The mixer will transfer your bitcoin from the deposit address into a big “house account” along with all the other bitcoin currently being mixed; and
Then, over some time the mixer will use the house account to dole out your bitcoin in smaller increments to the withdrawal addresses that you provided, possibly after deducting a fee.