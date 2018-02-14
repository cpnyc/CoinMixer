
# CoinMixer

A Bitcoin mixer is one way to maintain your privacy on the Bitcoin network.  Here’s how one popular mixer https://coinmixer.se/en/ works:

You provide  a list of new, unused addresses that you own to the mixer

The mixer provides you with a new deposit address that it owns

You transfer your bitcoins to that address

The mixer will detect your transfer by watching or polling the P2P Bitcoin network

The mixer will transfer your bitcoin from the deposit address into a big “house account” along with all the other bitcoin currently being mixed and

Then, over some time the mixer will use the house account to dole out your bitcoin in smaller increments to the withdrawal addresses that you provided, possibly after deducting a fee.




# Here are some logs from the code:

Mixer - Amount 12.5 transfered from Sender John to HouseAccount

Mixer - Mixing fee charged 0.001 to recipient Jacob

Mixer - Amount 9.874 transfered from HouseAccount to Recipient Jacob

Mixer - Mixing fee charged 0.0003 to recipient Jacob

Mixer - Amount 2.6247 transfered from HouseAccount to Recipient Jacob






Mixer - Amount 1 transfered from Sender John to HouseAccount

Mixer - Mixing fee charged 0 to recipient Jacob

Mixer - Amount 0.17 transfered from HouseAccount to Recipient Jacob

Mixer - Mixing fee charged 0.0001 to recipient Jacob

Mixer - Amount 0.8299 transfered from HouseAccount to Recipient Jacob






Mixer - Amount 0.01 transfered from Sender John to HouseAccount

Mixer - Mixing fee charged 0 to recipient Julie

Mixer - Amount 0.0086 transfered from HouseAccount to Recipient Julie

Mixer - Mixing fee charged 0 to recipient Julie

Mixer - Amount 0.0014 transfered from HouseAccount to Recipient Julie

# CoinMixer
