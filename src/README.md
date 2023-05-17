# Poznamky
## Casova slozitost Exhaustive Optimizeru
* $a$ je pocet startingTimes, $b$ je pocet allowedDurations, $S$ je mnozina shiftu a $I$ je mnozina uplne vsech incidentu pres vsechny incidentsSets.
* Pocet prohledanych ShiftPlanu je $(a \times b + 1)^{|S|}$
	* $a$ a $b$ udavaji vetvici faktor, protoze uvnitr jedne hladiny zkousim vsechny moznosti starting times a durations
	* +1, protoze shift jeste muze byt nepouzity (0s-0s)
	* $|S|$ udava hloubku stavoveho prostoru

Celkem odvedene prace je:
$$
(a \times b + 1)^{|S|} * |I|
$$

* Pro kazdy jeden shiftPlan musim spusti simulaci, ta je linearni vuci $|I|$

## TODO
* Benchmarkuj simulaci
* Zrefaktoruj step v simulaci at se shifts prochazi pouze jednou ne dvakrat
* nejaky chytrejsi optimizer

