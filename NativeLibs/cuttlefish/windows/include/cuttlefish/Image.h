/*
 * Copyright 2017-2024 Aaron Barany
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
 * @brief File containing the Image class to load and manipulate images.
 */

#include <cuttlefish/Config.h>
#include <cuttlefish/Export.h>
#include <cuttlefish/Color.h>
#include <cstddef>
#include <cstdint>
#include <iosfwd>
#include <memory>
#include <vector>

namespace cuttlefish
{

struct ColorRGBAd;

/**
 * @brief Class to store the data for and manipulate source images.
 *
 * Loading an image is the first step for creating a texture. It is loaded from a source image file
 * (such as a PNG) then transformed as desired, such as resizing or flipping the image. Afterward,
 * one or more images will be added to a Texture.
 *
 * @remark The coordinate (0, 0) is the upper-left of the image.
 */
class CUTTLEFISH_EXPORT Image
{
public:

	/**
	 * @brief Enum for the pixel format of the image.
	 */
	enum class Format
	{
		Invalid, ///< Invalid format, used for errors.
		Gray8,   ///< 8-bit grayscale.
		Gray16,  ///< 16-bit grayscale.
		RGB5,    ///< 6-bit per channel RGB.
		RGB565,  ///< 5, 6, and 5 bits per channel RGB.
		RGB8,    ///< 8-bit per channel RGB.
		RGB16,   ///< 16-bit per channel RGB.
		RGBF,    ///< Floating point RGB.
		RGBA8,   ///< 8-bit per channel RGBA.
		RGBA16,  ///< 16-bit per channel RGBA.
		RGBAF,   ///< Floating point RGBA.
		Int16,   ///< Signed 16-bit integer.
		UInt16,  ///< Unsigned 16-bit integer.
		Int32,   ///< Signed 32-bit integer.
		UInt32,  ///< Unsigned 32-bit integer.
		Float,   ///< Float.
		Double,  ///< Double.
		Complex  ///< Two doubles as a complex number.
	};

	/**
	 * @brief Enum for the filter to use when resizing.
	 */
	enum class ResizeFilter
	{
		Box,		///< Averages all pixels in the box.
		Linear,		///< Linear sampling.
		Cubic,		///< Cubic curve sampling.
		CatmullRom, ///< Catmull-Rom curve fitting.
		BSpline		///< B-Spline curve fitting.
	};

	/**
	 * @brief Enum for the angle to rotate by.
	 */
	enum class RotateAngle
	{
		CW90,	///< 90 degrees clockwise.
		CW180,	///< 180 degrees clockwise.
		CW270,	///< 270 degrees clockwise.
		CCW90,	///< 90 degrees counter-clockwise.
		CCW180, ///< 180 degrees counter-clockwise.
		CCW270	///< 270 degrees counter-clockwise.
	};

	/**
	 * @brief Enum for a color channel.
	 */
	enum class Channel
	{
		Red,   ///< Red channel
		Green, ///< Green channel
		Blue,  ///< Blue channel
		Alpha, ///< Alpha channel
		None   ///< Unused. Will be set to 1 for alpha, 0 otherwise.
	};

	/**
	 * @brief Bitmask enum for options when creating normal maps.
	 */
	enum class NormalOptions
	{
		Default = 0x0,	///< Default options.
		KeepSign = 0x1, ///< Keep the range in [-1, 1] rather than [0, 1].
		WrapX = 0x2,	///< Wrap along the X axis.
		WrapY = 0x4		///< Wrap along the Y axis.
	};

	Image();

	/**
	 * @brief Loads an image from file.
	 * @param fileName The name of the file to load.
	 * @param colorSpace The color space of the image.
	 * @remark The image will be invalid if it failed to load.
	 */
	explicit Image(const char *fileName, ColorSpace colorSpace = ColorSpace::Linear);

	/**
	 * @brief Loads an image from a stream.
	 * @param stream The stream to read from. This must be opened in binary mode!
	 * @param colorSpace The color space of the image.
	 * @remark The image will be invalid if it failed to load.
	 */
	explicit Image(std::istream &stream, ColorSpace colorSpace = ColorSpace::Linear);

	/**
	 * @brief Loads an image from data.
	 * @param data The data to load from.
	 * @param size The size of the data.
	 * @param colorSpace The color space of the image.
	 * @remark The image will be invalid if it failed to load.
	 */
	Image(const void *data, std::size_t size, ColorSpace colorSpace = ColorSpace::Linear);

	/**
	 * @brief Initializes an empty image.
	 * @param format The pixel format.
	 * @param width The width of the image.
	 * @param height The height of the image.
	 * @param colorSpace The color space of the image.
	 * @remark The image will be invalid if it failed to initialize.
	 */
	Image(Format format, unsigned int width, unsigned int height,
			ColorSpace colorSpace = ColorSpace::Linear);

	~Image();

	/// @cond
	Image(const Image &other);
	Image(Image &&other) noexcept;

	Image &operator=(const Image &other);
	Image &operator=(Image &&other) noexcept;
	/// @endcond

	/**
	 * @brief Returns whether or not an image is valid.
	 * @return True if valid.
	 */
	bool isValid() const;

	/** @copydoc isValid() */
	explicit operator bool() const;

	/**
	 * @brief Loads an image from file.
	 * @param fileName The name of the file to load.
	 * @param colorSpace The color space of the image.
	 * @return False if the image couldn't be loaded.
	 */
	bool load(const char *fileName, ColorSpace colorSpace = ColorSpace::Linear);

	/**
	 * @brief Loads an image from a stream.
	 * @param stream The stream to load from. This must be opened in binary mode!
	 * @param colorSpace The color space of the image.
	 * @return False if the image couldn't be loaded.
	 */
	bool load(std::istream &stream, ColorSpace colorSpace = ColorSpace::Linear);

	/**
	 * @brief Loads an image from data.
	 * @param data The data to load from.
	 * @param size The size of the data.
	 * @param colorSpace The color space of the image.
	 * @return False if the image couldn't be loaded.
	 */
	bool load(const void *data, std::size_t size, ColorSpace colorSpace = ColorSpace::Linear);

	/**
	 * @brief Loads an image from raw data in BGRA32.
	 * @param data The data to load from.
	 * @param size The size of the data.
	 * @param colorSpace The color space of the image.
	 * @return False if the image couldn't be loaded.
	 */
	bool loadRaw(const void *data, std::size_t size, int width, int height, ColorSpace colorSpace = ColorSpace::Linear);

	/**
	 * @brief Saves an image to a file.
	 *
	 * Nearly all image file formats, such as png, jpeg, etc., are supported. This will use the
	 * default parameters for settings like compression, and is intended for the most common use
	 * cases. For more intricate use cases where finer levels of control are needed, you will need
	 * to extract the raw image data and use a dedicated image library.
	 *
	 * @remark You should ensure that the image's pixel format is supported by the file format.
	 * @param fileName The name of the file to save.
	 * @return False if the image couldn't be saved.
	 */
	bool save(const char *fileName);

	/**
	 * @brief Saves an image to byte vector.
	 *
	 * Nearly all image file formats, such as png, jpeg, etc., are supported. This will use the
	 * default parameters for settings like compression, and is intended for the most common use
	 * cases. For more intricate use cases where finer levels of control are needed, you will need
	 * to extract the raw image data and use a dedicated image library.
	 *
	 * @remark You should ensure that the image's pixel format is supported by the file format.
	 * @param stream The stream to save the image to. This must be opened in binary mode!
	 * @param extension The file extension used to determine the file format.
	 * @return False if the image couldn't be saved.
	 */
	bool save(std::ostream &stream, const char *extension);

	/**
	 * @brief Saves an image to a stream.
	 *
	 * Nearly all image file formats, such as png, jpeg, etc., are supported. This will use the
	 * default parameters for settings like compression, and is intended for the most common use
	 * cases. For more intricate use cases where finer levels of control are needed, you will need
	 * to extract the raw image data and use a dedicated image library.
	 *
	 * @remark You should ensure that the image's pixel format is supported by the file format.
	 * @param[out] outData The byte vector to save the image to. The contents will be overwritten.
	 * @param extension The file extension used to determine the file format.
	 * @return False if the image couldn't be saved.
	 */
	bool save(std::vector<std::uint8_t> &outData, const char *extension);

	/**
	 * @brief Initializes an empty image.
	 * @param format The pixel format.
	 * @param width The width of the image.
	 * @param height The height of the image.
	 * @param colorSpace The color space of the image.
	 * @return False if the image couldn't be initialized.
	 */
	bool initialize(Format format, unsigned int width, unsigned int height,
					ColorSpace colorSpace = ColorSpace::Linear);

	/**
	 * @brief Resets the image to an unitialized state.
	 */
	void reset();

	/**
	 * @brief Gets the pixel format of the image.
	 * @return The pixel format.
	 */
	Format format() const;

	/**
	 * @brief Gets the color space of the image.
	 * @return The color space.
	 */
	ColorSpace colorSpace() const;

	/**
	 * @brief Gets the number of bits per pixel in the image.
	 * @return The bits per pixel.
	 */
	unsigned int bitsPerPixel() const;

	/**
	 * @brief Gets the width of the image.
	 * @return The width in pixels.
	 */
	unsigned int width() const;

	/**
	 * @brief Gets the height of the image.
	 * @return The height in pixels.
	 */
	unsigned int height() const;

	/**
	 * @brief Gets the mask for the red channel for a 16 or 32 BPP RGB or RGBA format.
	 * @return The red channel mask.
	 */
	std::uint32_t redMask() const;

	/**
	 * @brief Gets the bit shift for the red channel for a 16 or 32 BPP RGB or RGBA format.
	 * @return The red channel shift.
	 */
	unsigned int redShift() const;

	/**
	 * @brief Gets the mask for the green channel for a 16 or 32 BPP RGB or RGBA format.
	 * @return The green channel mask.
	 */
	std::uint32_t greenMask() const;

	/**
	 * @brief Gets the bit shift for the green channel for a 16 or 32 BPP RGB or RGBA format.
	 * @return The green channel shift.
	 */
	unsigned int greenShift() const;

	/**
	 * @brief Gets the mask for the blue channel for a 16 or 32 BPP RGB or RGBA format.
	 * @return The blue channel mask.
	 */
	std::uint32_t blueMask() const;

	/**
	 * @brief Gets the bit shift for the blue channel for a 16 or 32 BPP RGB or RGBA format.
	 * @return The blue channel shift.
	 */
	unsigned int blueShift() const;

	/**
	 * @brief Gets the mask for the alpha channel for a 16 or 32 BPP RGB or RGBA format.
	 * @return The alpha channel mask.
	 */
	std::uint32_t alphaMask() const;

	/**
	 * @brief Gets the bit shift for the alpha channel for a 16 or 32 BPP RGB or RGBA format.
	 * @return The alpha channel shift.
	 */
	unsigned int alphaShift() const;

	/**
	 * @brief Gets a scanline of the image.
	 * @param y The Y coordinate of the scanline.
	 * @return The data for the scanline.
	 */
	void *scanline(unsigned int y);

	/** @copydoc scanline() */
	const void *scanline(unsigned int y) const;

	/**
	 * @brief Gets a pixel as a floating point value.
	 * @remark This will work with any image format.
	 * @param[out] outColor The color of the pixel.
	 * @param x The X coordinate of the image.
	 * @param y The Y coordinate of the image.
	 * @return False if the pixel is out of range.
	 */
	bool getPixel(ColorRGBAd &outColor, unsigned int x, unsigned int y) const;

	/**
	 * @brief Sets a pixel as a floating point value.
	 * @remark This will work with any image format.
	 * @param x The X coordinate of the image.
	 * @param y The Y coordinate of the image.
	 * @param color The color of the pixel.
	 * @param convertGrayscale True to convert to grayscale for grayscale image types
	 *     (Gray8, Gray16, Float, Double), false to take the red channel as-is.
	 * @return False if the pixel is out of range.
	 */
	bool setPixel(unsigned int x, unsigned int y, const ColorRGBAd& color,
		bool convertGrayscale = true);

	/**
	 * @brief Converts the image to another pixel format.
	 * @param dstFormat The new pixel format.
	 * @param convertGrayscale True to convert to grayscale for grayscale image types
	 *     (Gray8, Gray16, Float, Double), false to take the red channel as-is.
	 * @return The converted image.
	 */
	Image convert(Format dstFormat, bool convertGrayscale = true) const;

	/**
	 * @brief Resizes an image.
	 * @remark In some situations the format may change.
	 * @remark Int*, UInt32, Double, and Complex types may only use Nearest and Linear filters.
	 * @param width The new width of the image.
	 * @param height The new height of the image.
	 * @param filter The filter to use for resizing.
	 * @return The resized image, or an invalid image if the format cannot be resized.
	 */
	Image resize(unsigned int width, unsigned int height, ResizeFilter filter) const;

	/**
	 * @brief Rotates an image.
	 * @param angle The angle to rotate by.
	 * @return The rotated image.
	 * @return The rotated image, or an invalid image if the format cannot be rotated.
	 */
	Image rotate(RotateAngle angle) const;

	/**
	 * @brief Flips the image horizontally in place.
	 * @return False if the image was invalid.
	 */
	bool flipHorizontal();

	/**
	 * @brief Flips the image vertically in place.
	 * @return False if the image was invalid.
	 */
	bool flipVertical();

	/**
	 * @brief Pre-multiplies the alpha values with the color values in place.
	 * @return False if the image was invalid.
	 */
	bool preMultiplyAlpha();

	/**
	 * @brief Converts the image to a different color space.
	 *
	 * This will affect every channel except the alpha channel.
	 *
	 * @remark Converting from sRGB to linear will reduce the precision for percieved color values,
	 *     so it's not recommended for 8 bits per channel or less.
	 * @return False if the image was invalid.
	 */
	bool changeColorSpace(ColorSpace colorSpace);

	/**
	 * @brief Converts to grayscale.
	 * @return False if the image was invalid.
	 */
	bool grayscale();

	/**
	 * @brief Swizzles the texture.
	 * @param red The channel to place in red.
	 * @param green The channel to place in green.
	 * @param blue The channel to place in blue.
	 * @param alpha The channel to place in alpha.
	 * @return False if the image was invalid.
	 */
	bool swizzle(Channel red, Channel green, Channel blue, Channel alpha);

	/**
	 * @brief Creates a normal map from the R channel of the image.
	 * @param options The options to use for computing the normal map.
	 * @param height The height for the image.
	 * @param dstFormat The format of the final image.
	 * @return The normal map image.
	 */
	Image createNormalMap(NormalOptions options = NormalOptions::Default, double height = 1.0,
		Format dstFormat = Format::RGBF);

	private:
		struct Impl;
		std::unique_ptr<Impl> m_impl;
	};

	/**
	 * @brief Combines two normal options.
	 * @param left The left options.
	 * @param right The right options.
	 * @return The combination of the left and right options.
	 */
	inline Image::NormalOptions operator|(Image::NormalOptions left, Image::NormalOptions right)
	{
		return static_cast<Image::NormalOptions>(static_cast<unsigned int>(left) |
												 static_cast<unsigned int>(right));
	}

	/**
	 * @brief Combines two normal options.
	 * @param left The left options to add right options to.
	 * @param right The right options.
	 * @return The combination of the left and right options.
	 */
	inline Image::NormalOptions &operator|=(Image::NormalOptions &left, Image::NormalOptions right)
	{
		left = static_cast<Image::NormalOptions>(static_cast<unsigned int>(left) |
												 static_cast<unsigned int>(right));
		return left;
	}

	/**
	 * @brief Checks if there are shared options between two normal options.
	 * @param left The left options.
	 * @param right The right options.
	 * @return True if there are shared options between left and right.
	 */
	inline bool operator&(Image::NormalOptions left, Image::NormalOptions right)
	{
		return (static_cast<unsigned int>(left) & static_cast<unsigned int>(right)) != 0;
	}

} // namespace cuttlefish
