# Osnova
Osnova
Introduction
* uvede cloveka do problematky - jakou mame motivaci a jake mame cile - jednoduse

Popis problemu
* vice do hloubky

Popis Optimization problemu
* teorie obecne
* vsechny terminy
* budu se pak z tohohle odrazet
* muzu tam dat i neco co treba nebudu primo pouzivat, zalezi jak je to zajimave / provazane s tim co realne delam

Popis simulace
* nejaky introduce do problemu urcite

Jen prospesne pokud pridam i nejakou kapitolu na popis moji architektury toho kodu, designu, paralelizace nebo nejaka optimalizace, popis i mapovani te domeny do mych struktur, casova a pametova slozitost
* jak to je nejak naprogramovane

Vysledky
* porovnani tech pristupu, nejake grafy, jak co rychle konverguje
* zkouset ruzne situace, i extremni, napr. strasne moc nemocnic, depotu, blizko u sebe, hrozne moc incidentu / shiftu atd ...
	* sledovat jak se jednotlive metody chovaji v techle situacich

Conclusion
* finalni porovnani, asi i nejaky ?vlastni nazor?, co je nejlepsi podle me

40-60 stranek, 30 je malo.
Kod neni tak dulezity. Dulezity je hlavne ten text.

## Tipy
Prvni napsat ten kod a pohrat si s temi metodami atd ... az pak spise zacit psat. Pomuze mi to i pri tom co dat do te faze teorie a jak to cele uvest.
Nemuzu delat uvod do neceho co jeste nevim jak bude probihat.

# Napady co udelat
## Zoptimalizovat simulaci, musi byt fakt mega rychla
* a zatim je spise pomalejsi
100 sanitek a 2000 incidentu -> 20 sekund ... to je nejak moc
50 sanitek a 200 incidentu -> 6 sekund podle benchmark, ale to mi prijde hodne, ze to bude mene, ale i tak ...

1. Distance matrik pro nejblizsi nemocnici z nejake zdiskretizovane mapy?
1. cahovani plannable incidentu?

## Stochastic search nepotrebuju
Protoze eval function mam na ruce.

## Prohledavni pres sousedy 
1. Hill climb
	* prilis stupidni, nebudu delat

1. Tabu search
	* jak zvolit velikost tabulky?
		* odpovida stavum kam se nevratim v _velikost tabulky_ krocich, takze asi odvodit nejak z poctu shiftu??
			* podivat se na velikost tabulky u reseni jinych problemu - sudoku napr.
	* muzu zacit s nevalidnim resenim?
		* muzu!
	* jak udelat damping, at nevracim jen nekonecno?
		* jak zachytit, ze i kdyz shift plan neni validni, tak ma pomerne slusny success rate napr., takze bude lepsi nez, skoro tak dobry jako validni, nez nejaky co ma success rate o dost nizsi?

1. Simulated annealing
	* jak zvolit T? Mozna jako mean random 200 shiftPlanu? Bude ale hodne natahovat int.MaxValue, takze mozna median? 

Ve vsech pripadech je ale treba nejaky zpusob ziskani sousedu. To jsou shiftPlany, ktere jsou v nejakem smyslu podobne a musi byt validni.

## Co je soused
1. Stejny pocet shiftu
1. Prumerna celkova delka pres intervaly je stejna
1. stejny pocet shiftu se stejnymi delkami
	* permutovat shifty, neco jako v TSP jak ziskas sousedy
1. Brat v potaz prostor, ne jenom cas / delky intervalu

Samotny swapping atd nestaci. Je treba intervaly jeste nejak zkracovat.


### Zkracovani
1. Zkratim nejaky shift co nejmene to jde
	* bud zmenim starting time nebo end time, na nejblizsi nizsi dovoleny interval length
	* celkovy pocet je 2 * #shift
		* buz zkratim zleva nebo zprava, pro vybranou shiftu

## Evolucni
1. Geneticke


