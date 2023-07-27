-> main

=== main ===
Which weapon will you choose, <b><color=\#FF1E35>fool</color></b>? #speaker:Red cube #emotion:neutral #side:right
    + [Sword]
        -> chosen("the sword")
    + [Axe]
        -> chosen("the axe")
    + [Bow]
        -> chosen("the bow")
    + [Flail]
        -> chosen("the flail")
        
=== chosen(weapon) ===
You chose {weapon}!
This should be a <sp:10>sloooooow text.
This should be a <sp:300>quicktextveryveryquickzoooooooooooooooom.
This.<p:long> Text. <p:long>Should. <p:long>Pause. <p:long>A lot.
This text should have <anim:wave>PIZZAZZ!</anim>
This text should be <anim:shake><b>FUMING WITH UNYIELDING RAGE</b></anim>
This line makes a sphere appear above my head! #event:0
This line makes a sphere appear next to me when I finish this line. <p:long> <sp:10> Riiight... <sp:150> Now! #event:1

-> END