Hey there, you're testing out the Virtual Novel dialogue type! #start:Player #emotion:neutral #enter:left #move:0,3 #start:Blue cube #emotion:neutral #enter:right #move:0,7 #speaker:Blue cube
-> main

=== main ===
So, whaddya wanna know about?
+ [Can you move to the right?]
    Yeah, sure, here I go! #speaker:Blue cube #move:0,8
    I can even go to the middle! #move:0,5
    And back where I was. #move:0,7
    -> main
+ [Show me your WAR FACE!]
    HERE'S MY WARFACE! #speaker:Blue cube #emotion:angry
    and here's the face when i'm scared hahaha go away #emotion:fear
    But generally I remain like this. #emotion:neutral
    -> main
+ [Change your name to "Best cube".]
    Totally, man. Here goes. #speaker:Blue cube #aka:Best cube
    I could get used to this, although...
    Let's go back to my previous name. #aka:Blue cube
    -> main
+ [FLY YOU FOOL]
    GANDALF NOOOOOO #speaker:Blue cube #flip:right #exit:right
    OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO
    OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO-I'm back. #flip:left #show:show #move:0,7
    -> main
+ [Flip and hide yourself!]
    Sure thing, here goes! #speaker:Blue cube
    Flip! #flip:right
    Flip! #flip:left
    Hide! #show:hide
    Show! #show:show
    -> main
+ [All right, that'll be all.]
    No problem mate, happy to help. #speaker:Blue cube #emotion:happy
    -> END