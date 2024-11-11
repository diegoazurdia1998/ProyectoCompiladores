45+3

Stack								Input		Action
0								    45 + 3	$	shift 5
0 num(45) 5							+ 3$		reduce 6
0 <F>(value = 45)						+ 3$		goto 3
0 <F>(value = 45) 3						+ 3$		reduce 4
0 <T>(value = 45) 						+ 3$		goto 2
0 <T>(value = 45) 2 					+ 3$		reduce 2
0 <E>(value = 45)  						+ 3$		goto 1
0 <E>(value = 45) 1						+ 3$		shift 6
0 <E>(value = 45) 1 + 6					3$		shift 5
0 <E>(value = 45) 1 + num(3) 5				$		reduce 6
0 <E>(value = 45) 1 + <F>(value = 3)			$		goto 3
0 <E>(value = 45) 1 + <F>(value = 3) 3 			$		reduce 4
0 <E>(value = 45) 1 + <T>(value = 3) 			$		goto 2
0 <E>(value = 45) 1 + <T>(value = 3) 2 			$		reduce 2
0 <E>(value = 45) 1 + <E>(value = 3)  			$		no hay E + E no aceptada

2 + 3 * 4
Stack								Input		Action
0 								2 + 3 * 4	shift 5
0 num(2) 5							+ 3 * 4 	reduce 6
0 <F>(value=2)						+ 3 * 4		goto 3
0 <F>(value=2) 3 						+ 3 * 4		reduce 4
0 <T>(value=2) 						+ 3 * 4 	goto 2
0 <T>(value=2) 2						+ 3 * 4		reduce 2
0 <E>(value=2)						+ 3 * 4		goto 1
0 <E>(value=2) 1						+ 3 * 4		shift 6
0 <E>(value=2) + 6						3 * 4 		shift 5
0 <E>(value=2) + 6 num(3) 5 				* 4 		reduce 6
0 <E>(value=2) + 6 <F>(value=3) 			* 4		goto 3
0 <E>(value=2) + 6 <F>(value=3) 3			* 4		reduce 4
0 <E>(value=2) + 6 <T>(value=3) 			* 4		goto 9
0 <E>(value=2) + 6 <T>(value=3) 9 			* 4		shift 7
0 <E>(value=2) + 6 <T>(value=3) 9 * 7 			4		shift 5
0 <E>(value=2) + 6 <T>(value=3) 9 * num(4) 5		$		reduce 6
0 <E>(value=2) + 6 <T>(value=3) 9 * <F>(value=4) 	$		reduce 1
0 <E>(value=5) * <F>(value=4)				$		



SERIE 3
_t0 = b + c;
a = _t0 + b;
d = b + c;
_t1 = a * a;
_t2 = b * c;
_t3 = b + c; -> redundante, se puede usar d que tiene la misma operación en lugar de t3
b = _t1 + _t2;


a = _t0 + b;
d = b + c;
_t1 = a * a;
_t2 = b * c;
b = _t1 + _t2;



SERIE 2
ppublic class FormulaCuadratica {

public static double[] calcularRaices(int a, int b, int c) {
        double discriminante = Math.pow(b, 2) - 4 * a * c;
        double[] raices;

        if (discriminante > 0) {
            raices = new double[2];
            raices[0] = (-b + Math.sqrt(discriminante)) / (2 * a);
            raices[1] = (-b - Math.sqrt(discriminante)) / (2 * a);
        } else if (discriminante == 0) {
            raices = new double[1];
            raices[0] = -b / (2.0 * a);
        } else { 
            return null;
        }
        return raices;
    }

    public static void main(String[] args) {
        int a = 1, b = -3, c = 2;
        double[] resultado = calcularRaices(a, b, c);

        if (resultado != null) {
            if (resultado.length == 2) {
                System.out.println("Raíz 1: " + resultado[0]);
                System.out.println("Raíz 2: " + resultado[1]);
            } else {
                System.out.println("Raíz doble: " + resultado[0]);
            }
        } else {
            System.out.println("La ecuación no tiene soluciones reales.");
        }
    }
}

codigo 3 direcciones

t1 = b * b
t2 = 4 * a
t3 = t2 * c
t4 = t1 - t3
t1 = sqrt(t4)
t2 = -1 * b
t3 = t2 + t1   // parte positiva
t1 = 2 * a
t2 = t3/t1

t1 = b * b
t2 = 4 * a
t3 = t2 * c
t4 = t1 - t3
t1 = sqrt(t4)
t2 = -1 * b
t3 = t2 - t1   // parte negativa
t1 = 2 * a
t2 = t3/t1





















