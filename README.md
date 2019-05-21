# C# GNFS

C# .NET implementation of the general number field sieve for factoring very large semi-prime numbers, which are used by the RSA public key encryption algorithm. The general number field sieve is an involved process, consisting of many steps: a polynomial creation, testing and selection process, finding 'relations' who's algebraic and rational norms are smooth with respect to an algebraic and rational factor base, the creation, and solving of a (very) large (0,1)-matrix to find sets of relations where the product of all the norms in that set are a perfect square number, finding the square roots of polynomials over a finite field, the Chinese remainder theorem and finally the testing of several found candidates for a non-trivial difference of squares. 

Did you understood all that? Good. Because it's * just * that easy, folks! &nbsp; /s

# Screenshots

![Screenshot](https://github.com/AdamWhiteHat/GNFS/blob/master/ScreenShot_001.png "GNFS Application Screenshot")


# Literature & Publications

Below is some literature on the subject of General Number Field Sieves that helped guide me during the construction of this project. Most of the code was written after reading Per Leslie Jensen's paper. However, the final step, finding the square root of a polynomial over a finite field (or called simply the 'square root step') was the most challenging step because it required an understanding of abstract algebra, and Per Leslie Jensen's paper was almost completely void of any details pertaining to this step. I used Matthew E. Briggs's paper to help guide me in writing the code for the final step. 

* [Factoring integers with the number field sieve - J.P. Buhler, H.W Lenstra, Jr., and Carl Pomerance](https://github.com/AdamWhiteHat/GNFS/blob/master/Factoring%20integers%20with%20the%20number%20field%20sieve%20-%20J.P.%20Buhler%2C%20H.W%20Lenstra%2C%20Jr.%2C%20and%20Carl%20Pomerance.pdf)
* [An Introduction to the General Number Field Sieve - Matthew E. Briggs](https://github.com/AdamWhiteHat/GNFS/blob/master/An%20Introduction%20to%20the%20General%20Number%20Field%20Sieve%20-%20Matthew%20E.%20Briggs.pdf)
* [Integer Factorization - Master Thesis - Per Leslie Jensen](https://github.com/AdamWhiteHat/GNFS/blob/master/Integer%20Factorization%20-%20Master%20Thesis%20-%20Per%20Leslie%20Jensen.pdf)
* [The development of the number field sieve - H. W. Lenstra, Arjen K. Lenstra, Hendrik W. Lenstra](https://github.com/AdamWhiteHat/GNFS/blob/master/The%20development%20of%20the%20number%20field%20sieve%20-%20H.%20W.%20Lenstra%2C%20Arjen%20K.%20Lenstra%2C%20Hendrik%20W.%20Lenstra.djvu)
* [A General Number Field Sieve Implementaion - DJ Bernstein & Lenstra](https://github.com/AdamWhiteHat/GNFS/blob/master/A%20General%20Number%20Field%20Sieve%20Implementaion%20-%20DJ%20Bernstein%20%26%20Lenstra.PDF)
* [The Multiple Polynomial Quadradic Sieve - R.D. Silverman](https://github.com/AdamWhiteHat/GNFS/blob/master/The%20Multiple%20Polynomial%20Quadradic%20Sieve%20-%20R.D.%20Silverman.pdf)
* [The Number Field Sieve - C. Pomerance](https://github.com/AdamWhiteHat/GNFS/blob/master/The%20Number%20Field%20Sieve%20-%20C.%20Pomerance.pdf)


