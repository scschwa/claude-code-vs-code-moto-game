using System;
using UnityEngine;

namespace DesertRider.Audio
{
    /// <summary>
    /// Simple FFT (Fast Fourier Transform) implementation using Cooley-Tukey algorithm.
    /// Used for pre-analysis of MP3 files to detect beats and calculate intensity.
    /// Based on standard FFT algorithm with Unity optimization.
    /// </summary>
    public static class SimpleFFT
    {
        /// <summary>
        /// Performs FFT on real-valued input samples.
        /// Input size must be a power of 2 (512, 1024, 2048, etc.).
        /// </summary>
        /// <param name="samples">Input audio samples (must be power of 2 length).</param>
        /// <returns>Array of frequency magnitudes (length = samples.Length / 2).</returns>
        public static float[] Compute(float[] samples)
        {
            int n = samples.Length;

            // Validate input size is power of 2
            if (n == 0 || (n & (n - 1)) != 0)
            {
                Debug.LogError($"SimpleFFT: Input size must be power of 2, got {n}");
                return new float[n / 2];
            }

            // Convert to complex numbers (imaginary part = 0)
            Complex[] complex = new Complex[n];
            for (int i = 0; i < n; i++)
            {
                complex[i] = new Complex(samples[i], 0);
            }

            // Perform FFT
            FFT(complex, false);

            // Calculate magnitudes (only first half, due to symmetry)
            float[] magnitudes = new float[n / 2];
            for (int i = 0; i < n / 2; i++)
            {
                magnitudes[i] = complex[i].Magnitude;
            }

            return magnitudes;
        }

        /// <summary>
        /// Applies Hamming window to reduce spectral leakage.
        /// Call this before FFT for better frequency resolution.
        /// </summary>
        /// <param name="samples">Audio samples to window (modified in place).</param>
        public static void ApplyHammingWindow(float[] samples)
        {
            int n = samples.Length;
            for (int i = 0; i < n; i++)
            {
                float window = 0.54f - 0.46f * Mathf.Cos(2f * Mathf.PI * i / (n - 1));
                samples[i] *= window;
            }
        }

        /// <summary>
        /// Cooley-Tukey FFT algorithm (in-place, recursive).
        /// </summary>
        /// <param name="buffer">Complex number buffer (power of 2 length).</param>
        /// <param name="inverse">True for inverse FFT, false for forward FFT.</param>
        private static void FFT(Complex[] buffer, bool inverse)
        {
            int n = buffer.Length;
            if (n <= 1) return;

            // Divide
            Complex[] even = new Complex[n / 2];
            Complex[] odd = new Complex[n / 2];
            for (int i = 0; i < n / 2; i++)
            {
                even[i] = buffer[i * 2];
                odd[i] = buffer[i * 2 + 1];
            }

            // Conquer
            FFT(even, inverse);
            FFT(odd, inverse);

            // Combine
            double angle = (inverse ? 1.0 : -1.0) * 2.0 * Math.PI / n;
            for (int k = 0; k < n / 2; k++)
            {
                Complex wk = Complex.FromPolarCoordinates(1.0, angle * k);
                Complex t = wk * odd[k];

                buffer[k] = even[k] + t;
                buffer[k + n / 2] = even[k] - t;
            }

            // Normalize for inverse transform
            if (inverse)
            {
                for (int i = 0; i < n; i++)
                {
                    buffer[i] /= 2.0;
                }
            }
        }

        /// <summary>
        /// Complex number structure for FFT calculations.
        /// </summary>
        private struct Complex
        {
            public double Real;
            public double Imaginary;

            public Complex(double real, double imaginary)
            {
                Real = real;
                Imaginary = imaginary;
            }

            public float Magnitude
            {
                get { return (float)Math.Sqrt(Real * Real + Imaginary * Imaginary); }
            }

            public static Complex FromPolarCoordinates(double magnitude, double phase)
            {
                return new Complex(
                    magnitude * Math.Cos(phase),
                    magnitude * Math.Sin(phase)
                );
            }

            public static Complex operator +(Complex a, Complex b)
            {
                return new Complex(a.Real + b.Real, a.Imaginary + b.Imaginary);
            }

            public static Complex operator -(Complex a, Complex b)
            {
                return new Complex(a.Real - b.Real, a.Imaginary - b.Imaginary);
            }

            public static Complex operator *(Complex a, Complex b)
            {
                return new Complex(
                    a.Real * b.Real - a.Imaginary * b.Imaginary,
                    a.Real * b.Imaginary + a.Imaginary * b.Real
                );
            }

            public static Complex operator /(Complex a, double scalar)
            {
                return new Complex(a.Real / scalar, a.Imaginary / scalar);
            }
        }
    }
}
