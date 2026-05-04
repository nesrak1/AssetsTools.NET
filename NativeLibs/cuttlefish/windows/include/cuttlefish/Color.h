/*
 * Copyright 2017-2025 Aaron Barany
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#pragma once

/**
 * @file
 * @brief File containing structures for various color fromats.
 *
 * Colors that have 8-bit per channel or less may have different storage orders based on the
 * platform and source image. Use the shift and mask accessors in Image to access the color
 * channels. For example, for a 16 BPP image you can use the shift and mask on a 16-bit integer.
 * For a 24 or 32 BPP image, you can divide the shift by 8 to use as an index or use the shift and
 * mask on a 32-bit integer.
 */

#include <cuttlefish/Config.h>
#include <cmath>
#include <cstdint>

namespace cuttlefish
{

/**
* @brief Enum for the color space.
*/
enum class ColorSpace
{
	Linear, ///< Linear color space.
	sRGB    ///< sRGB color space.
};

/**
 * @brief Structure containing a 3 channel color with 16 bits per channel.
 */
struct ColorRGB16
{
	/**
	 * @brief Default constructor, leaving the members uninitialized.
	 */
	ColorRGB16() = default;

	/**
	 * @brief Constructs this with the individual channels.
	 * @param red The red channel.
	 * @param green The green channel.
	 * @param blue The blue channel.
	 */
	ColorRGB16(std::uint16_t red, std::uint16_t green, std::uint16_t blue)
		: r(red), g(green), b(blue)
	{
	}

	std::uint16_t r; ///< @brief The red channel.
	std::uint16_t g; ///< @brief The green channel.
	std::uint16_t b; ///< @brief The blue channel.
};

/**
 * @brief Structure containing a 4 channel color with 16 bits per channel.
 */
struct ColorRGBA16
{
	/**
	 * @brief Default constructor, leaving the members uninitialized.
	 */
	ColorRGBA16() = default;

	/**
	 * @brief Constructs this with the individual channels.
	 * @param red The red channel.
	 * @param green The green channel.
	 * @param blue The blue channel.
	 * @param alpha The alpha channel.
	 */
	ColorRGBA16(std::uint16_t red, std::uint16_t green, std::uint16_t blue, std::uint16_t alpha)
		: r(red), g(green), b(blue), a(alpha)
	{
	}

	std::uint16_t r; ///< @brief The red channel.
	std::uint16_t g; ///< @brief The green channel.
	std::uint16_t b; ///< @brief The blue channel.
	std::uint16_t a; ///< @brief The alpha channel.
};

/**
 * @brief Structure containing a 3 channel floating point color.
 */
struct ColorRGBf
{
	/**
	 * @brief Default constructor, leaving the members uninitialized.
	 */
	ColorRGBf() = default;

	/**
	 * @brief Constructs this with the individual channels.
	 * @param red The red channel.
	 * @param green The green channel.
	 * @param blue The blue channel.
	 */
	ColorRGBf(float red, float green, float blue)
		: r(red), g(green), b(blue)
	{
	}

	float r; ///< @brief The red channel.
	float g; ///< @brief The green channel.
	float b; ///< @brief The blue channel.
};

/**
 * @brief Structure containing a 4 channel floating point color.
 */
struct ColorRGBAf
{
	/**
	 * @brief Default constructor, leaving the members uninitialized.
	 */
	ColorRGBAf() = default;

	/**
	 * @brief Constructs this with the individual channels.
	 * @param red The red channel.
	 * @param green The green channel.
	 * @param blue The blue channel.
	 * @param alpha The alpha channel.
	 */
	ColorRGBAf(float red, float green, float blue, float alpha)
		: r(red), g(green), b(blue), a(alpha)
	{
	}

	float r; ///< @brief The red channel.
	float g; ///< @brief The green channel.
	float b; ///< @brief The blue channel.
	float a; ///< @brief The alpha channel.
};

/**
 * @brief Structure containing a 4 channel double-precision floating point color.
 */
struct ColorRGBAd
{
	/**
	 * @brief Default constructor, leaving the members uninitialized.
	 */
	ColorRGBAd() = default;

	/**
	 * @brief Constructs this with the individual channels.
	 * @param red The red channel.
	 * @param green The green channel.
	 * @param blue The blue channel.
	 * @param alpha The alpha channel.
	 */
	ColorRGBAd(double red, double green, double blue, double alpha)
		: r(red), g(green), b(blue), a(alpha)
	{
	}

	double r; ///< @brief The red channel.
	double g; ///< @brief The green channel.
	double b; ///< @brief The blue channel.
	double a; ///< @brief The alpha channel.
};

/**
 * @brief Structure containing a complex number.
 */
struct Complex
{
	/**
	 * @brief Default constructor, leaving the members uninitialized.
	 */
	Complex() = default;

	/**
	 * @brief Constructs this with the individual components.
	 * @param real The real component.
	 * @param imaginary The imaginary component.
	 */
	Complex(double real, double imaginary)
		: r(real), i(imaginary)
	{
	}

	double r; ///< @brief The real component.
	double i; ///< @brief The imaginary component.
};

/**
 * @brief Converts a color to grayscale.
 * @param r The red channel.
 * @param g The green channel.
 * @param b The blue channel.
 * @return The grayscale value.
 */
inline double toGrayscale(double r, double g, double b)
{
	// Rec. 709
	return r*0.2126 + g*0.7152 + b*0.0722;
}

/**
 * @brief Converts a color channel from sRGB to linear color space.
 * @param c The color channel in sRGB space.
 * @return The color channel in linear space.
 */
inline double sRGBToLinear(double c)
{
	if (c <= 0.04045)
		return c/12.92;
	return std::pow((c + 0.055)/1.055, 2.4);
}

/**
 * @brief Converts a color channel from linear to sRGB color space.
 * @param c The color channel in linear space.
 * @return The color channel in sRGB space.
 */
inline double linearToSRGB(double c)
{
	// linear to sRGB
	if (c <= 0.0031308)
		return c*12.92;
	return 1.055*std::pow(c, 1.0/2.4) - 0.055;
}

} // namespace cuttlefish
