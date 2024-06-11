using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TODO : MonoBehaviour
{
    /*
     * Cheap way to store general ideas and TODOs
     * 
     * TODO: make a bunch of generic endless waves as SOs and inject them into levels after normal waves are done
     * This will open up the idea of big earnings so need to think about balanced costs of upgrades etc
     * 
     * IDEA: make the holographic background speed up the more enemies are on screen
     * make old 70s80s arcade style beep sounds for sell/upgrade lives lost etc
     * IDEA: allow surplus level cash to go into a bank which can be used to buy silly things, like needless censoring enemies; pixellated graphics
     *change the theme to napkin sketch art; etc
     * 
     * IDEA: A space invaders level where the enemy starts at the top left and mazes its way down to the base at the bottom. You're only allowed rockets and the bases are at the bottom
     * IDEA: A music level where the bulets sounds are swapped out for tones
     * 
     * TWO FORMS : one aimed at a more mobile experience and the other with a fixed screen resolution and more elaborate story style game
     * similar to bloons, have a selectection of maps and certain game modes. Need to think about having some way to put towers anywhere
     * 
     * Main game idea: Stacks
     * You are charged with organising the defence of strategic locations.
     * Your main arsenal consists of building towers on predetermined suitable locations
     * You can choose from several tower bases which allow a different variety of stackable guns; 
     * Each stack may consist of several guns or a single gun for that layet
     * Some tower bases allow multiple stacks, others less layers but more guns per layer, etc.
     * Art style is based on the flakturm ceiling port holes idea
     * 
     * Or...stacks meaning mankind has retreated to living underground, beneath a series of levels or layers of piping.
     * The alien invaders scour each level looking for the hole to advance further down into the depths ultimately to find the human haven and destroy it.
     * These level stacks must be defended, by brave soldiers and engineers: engineers to fashion the ammunition and collect it from the battlefield between waves
     * and the soldiers who fight during the wave and recuperate in the rest period. a mismatch of how many soldiers/engineers you have may mean some work double shift
     * with detriments to the next waves performance, or rest collections performance. retreat is sometimes a better option. allow several to pass through but no spores
     * as they'll quickly overrun the next layer. each level can allow a few through to be picked off by the rookies below before a full level retreat is necessary>>>allows
     * for believable story repeating levels.
     * This would require pathfinding and movable 'towers' who are essentially archers as gun ammunition is not viable
     * engineers who fashion enough bolts/arrows for the next wave, during a wave too; and to erect and maintain path blocking structures
     * 
     * IDEA: train tracks to wheel around the towers with circuits, buildable by player, adds strategy to course flow
     * 
     * how about instead of having a life counter, we have a creeps on screen counter;
     * the idea being every few seconds a creep is added, and if a certain amount is reached, say 20 on screen at once
     * the game is over. not too original but a twist perhaps
     * there is no 'exit' point as such just a forever circuit the creeps infintely go round on
     * rewards still the same etc
     * maybe make it a game mode rather than anything game defining
     * nice/cheaty way to minimise cpu usage etc. but hopefully still provide that ramp up feeling of power and tension etc for the player
     * 
     * i like the idea of stacking towers but as it is (unlinked stacks) is just a cheap way of providing more build spots
     * to allow a more strategic implementaiton each buildspot height determiner should have effects on the tower built at that height
     * plus towers above/below each other should have shared/stolen buffs or effects maybe. ideas to explore.
     * 
     * General TODO
     * * Add a meta game power building screen;perks basically
     * collecting cash in game> save some for after the game to convert into research to allow improved world stats
     * save cash for meta game or spend in game to progress further and hopefully earn more cash  - tension of decision and balance
     * some possible perks: general damage increase; add preset number of extra buildspots; general towerpowers stats increase; more starting cash; more earning cash
     * 
     * *consolidate the artstyle for towers: main idea they construct vertically so the central 'pole' should be similar; only the barrel change 
     * visually. to start with maybe a simple circle with different color/thickness barrels. separate pole and barrel in prefab container probably best
     * 
     * * implement different tracks with obstacles/bullet shadow areas: maybe have destructable barriers, require min hits to disappear
     * 
     * * implement different towers: include the usual but have a good think about something new or unique
     * decide on a theme and maybe use that to inspire original stuff
     * 
     * * let towers level up on their own to grant expgains, as well as general upgrades 
     * * possibly allow a gem every ten tower level ups or something like that
     * 
     *DONE: 
     * 
     * 
     * * Think about including path finding, blocking etc
     * 
     * 
     * * * GENERAL BUGS:
     * 
     * FIXED: corrupted buttons on android menu screen were result of incorrect material for button images. it doesn't matter what texture you use but you seem to need to choose None for material
     * I was using Sprite-Default material and it was resulting in corrupted images. Good to know.
     * 
     * BUG: ice towers somtimes stop slowing targets after a while
     * 
     * 
     * TODO: isolate functionality of range visible and things like that using a broadcast system maybe? event/delegates
     * TODO: investigate if tonally discrete ice tinkling sound effects might be nice for some enemies; frozen perhaps?
     * with maybe some bass sounds for other creeps to create a sound spectrum 
     */

}
