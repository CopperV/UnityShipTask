# Zadanie rekrutacyjne - Opracowanie Dema systemu sterowania łódką

---

## 0. Prezentacja projektu

[![Prezentacja projektu](https://img.youtube.com/vi/2rObw8arBeI/maxresdefault.jpg)](https://www.youtube.com/watch?v=2rObw8arBeI)

---

## 1. Ogólny zamysł

Poruszanie się łódką bazuje na sile generowanej przez żagiel oraz opór stawiany przez łódkę (żagiel + kadłub).

Proces stojący za liczenie i poruszanie łódki można podzielić na kroki:

1. Z WindControllera pobieramy siłę oraz kierunek wiatru w obecnej pozycji
2. Następnie liczymy wiatr względny (wiatr - prędkość łódki)
3. Na podstawie kąta natarcia żagla oblicza się siłę nośną (Lift) oraz siłę oporu (Drag)
4. Dodajemy opór kadłuba
5. W obliczeniach uwzględniamy masę łódki - **siła / masa = przyspieszenie**
6. Przyspieszenie zmieniamy na zmianę prędkości, a prędkość na przesunięcie

Przyspieszenie łódki wynika wprost z drugiej zasady dynamiki Newtona:

$$
\vec{a} = \frac{\vec{F}}{m}
$$

---

## 2. Wiatr

Wiatr jest generowany przez `WindController`. Składa się z kilku warstw:

- **Kierunek startowy** – powoli się zmienia (szum Perlina).
- **Startowa siła** – również delikatnie faluje.
- **Porywy (gusts)** – losowe, krótkotrwałe wzmocnienia siły + lekka zmiana kierunku.
- **Turbulencja przestrzenna** – wiatr w różnych miejscach mapy trochę się różni (dzięki temu dwie łódki obok siebie mogą mieć nieco inny wiatr).

Metoda `GetWindForceAtPoint(position)` zwraca wektor wiatru w danym punkcie świata.

---

## 3. Siły na żaglu (aerodynamika)

### 3.1. Wiatr względny

Żagiel nie reaguje na wiatr bezwzględny, tylko na **wiatr względny**:

$$
\vec{v}_{rel} = \vec{v}_{wiatr} - \vec{v}_{łodzi}
$$

Im szybciej płynie łódka, tym bardziej zmienia się kierunek i siła wiatru, który „widzi” żagiel.  
Dlatego przy ostrym kursie pod wiatr wiatr względny jest mocno przesunięty do przodu.

### 3.2. Kąt natarcia (Angle of Attack)

Kąt natarcia $\alpha$ to kąt między cięciwą żagla a kierunkiem wiatru względnego.

Od tego kąta zależą współczynniki aerodynamiczne.

### 3.3. Ciśnienie dynamiczne

Podstawowa wielkość w aerodynamice:

$$
q = \frac{1}{2} \rho v_{rel}^{2}
$$

gdzie:
- $\rho$ – gęstość powietrza (≈ 1,225 kg/m³)
- $v_{rel}$ – prędkość wiatru względnego

### 3.4. Siła nośna i siła oporu

Klasyczne wzory:

$$
L = C_L(\alpha) \cdot q \cdot S
$$

$$
D = C_D(\alpha) \cdot q \cdot S
$$

gdzie:
- $L$ – siła nośna (Lift) – działa prostopadle do wiatru względnego
- $D$ – siła oporu (Drag) – działa wzdłuż wiatru względnego
- $S$ – powierzchnia żagli (m²)
- $C_L(\alpha)$, $C_D(\alpha)$ – współczynniki zależne od kąta natarcia

W kodzie stosujemy uproszczone przybliżenia współczynników:

$$
C_L = C_{Lmax} \cdot \sin(2\alpha)
$$

$$
C_D = C_{D0} + k \cdot \sin^{2}(\alpha)
$$

Dodatkowo siły są skalowane otwarciem żagla (0–100 %).

---

## 4. Opór kadłuba (woda)

Kadłub generuje opór, który hamuje łódkę.  
W projekcie stosujemy prosty model liniowy:

$$
F_{drag} = -k \cdot v
$$

Opór boczny jest wielokrotnie większy niż wzdłużny (mnożnik ~5,5–6,5).  
Dzięki temu łódka „trzyma się” kursu i nie zjeżdża mocno bokiem (leeway jest ograniczony).

---

## 5. Moment obrotowy (skręcanie)

Żagiel ustawiony pod kątem generuje moment, który obraca łódkę:

- Im bardziej żagiel jest wychylony na bok, tym większy moment.
- Im większa siła na żaglu, tym mocniej kręci.

To uproszczony model zjawiska znanego jako *weather helm* / *lee helm*.

Dodatkowo istnieje tłumienie prędkości kątowej (angular drag), żeby łódka nie kręciła się w nieskończoność.

---

## 6. Cały krok symulacji (podsumowanie)

```text
1. trueWind          = WindController.GetWindForceAtPoint(pozycja)
2. relativeWind      = trueWind - velocity
3. α                 = kąt między cięciwą żagla a relativeWind
4. q                 = ½ · ρ · |relativeWind|²
5. L, D              = C_L(α)·q·S , C_D(α)·q·S
6. sailForce         = L + D
7. hullDragForce     = opór kadłuba
8. totalForce        = sailForce + hullDragForce
9. acceleration      = totalForce / mass
10. velocity        += acceleration · Δt
11. position        += velocity · Δt
```

---

## 7. Kluczowe parametry (wartości dobrane do modelu w projekcie)

| Parametr              | Wartość     | Znaczenie                          |
|-----------------------|-------------|------------------------------------|
| Masa łodzi            | $480 kg$      | Bezwładność                        |
| Powierzchnia żagli    | $13,5 m^2$     | Wpływa na siłę ciągu               |
| Gęstość powietrza     | $1,225 kg/m^3$ | Stała fizyczna                     |
| Max prędkość          | $7,5 m/s$     | Ok. 14,5 węzła                     |
| Opór liniowy kadłuba  | $18 N/(m/s)$ | Hamowanie wzdłużne              |
| Mnożnik oporu bocznego| $5,5$     | Ogranicza znoszenie                |

---

## 8. Uproszczenia (ważne)

Model celowo pomija kilka zjawisk, ze względu na poświęcony czas jak i na potrzeby prostoty i stabilności:

- Brak przechyłu (heel) i stabilności poprzecznej
- Brak wpływu kształtu kadłuba na opór falowy w pełnej postaci
- Współczynniki $C_L$ i $C_D$ są uproszczonymi funkcjami (nie pochodzą z tunelu aerodynamicznego)
- Brak efektu slotu między fokiem a grotem
- Opór wody jest liniowy (w rzeczywistości przy wyższych prędkościach dominuje składnik $v^2$)
