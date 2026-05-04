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
 * @brief File containing the Texture class to set up and save a texture.
 */

#include <cuttlefish/Config.h>
#include <cuttlefish/Export.h>
#include <cuttlefish/Image.h>

#include <iosfwd>
#include <memory>
#include <unordered_map>
#include <vector>

namespace cuttlefish
{

/**
 * @brief Class describing a texture.
 *
 * A texture is created from one or more images, and are then converted to the final texture format.
 * From that point, it can be saved to a texture file.
 */
class CUTTLEFISH_EXPORT Texture
{
public:
	/**
	 * @brief Enum for the dimension of the texture.
	 */
	enum class Dimension
	{
		Dim1D, ///< 1D texture with only a width.
		Dim2D, ///< 2D texture.
		Dim3D, ///< 3D texture.
		Cube   ///< Cube map.
	};

	/**
	 * @brief Enum for the texture format.
	 */
	enum class Format
	{
		Unknown, ///< No known format.

		// Standard formats.
		A8,
		R4G4,         ///< RG 4 bits each.
		R4G4B4A4,     ///< RGBA 4 bits each.
		B4G4R4A4,     ///< BGRA 4 bits each.
		A4R4G4B4,     ///< ARGB 4 bits each.
		R5G6B5,       ///< RGB with 5, 6, 5 bits.
		B5G6R5,       ///< BGR with 5, 6, 5 bits.
		R5G5B5A1,     ///< RGBA with 5, 5, 5, 1 bits.
		B5G5R5A1,     ///< BGRA with 5, 5, 5, 1 bits.
		A1R5G5B5,     ///< ARGB with 1, 5, 5, 5 bits.
		R8,           ///< R with 8 bits.
		R8G8,         ///< RG with 8 bits each.
		R8G8B8,       ///< RGB with 8 bits each.
		B8G8R8,       ///< BGR with 8 bits each.
		R8G8B8A8,     ///< RGBA with 8 bits each.
		B8G8R8A8,     ///< BGRA with 8 bits each.
		A8R8G8B8,     ///< ARGB with 8 bits each.
		A8B8G8R8,     ///< ABGR with 8 bits each.
		A2R10G10B10,  ///< ARGB with 2, 10, 10, 10 bits.
		A2B10G10R10,  ///< ABGR with 2, 10, 10, 10 bits.
		R16,          ///< R with 16 bits.
		R16G16,       ///< RG wtih 16 bits each.
		R16G16B16,    ///< RGB with 16 bits each.
		R16G16B16A16, ///< RGBA with 16 bits each.
		R32,          ///< R with 32 bits.
		R32G32,       ///< RG with 32 bits each.
		R32G32B32,    ///< RGB with 32 bits each.
		R32G32B32A32, ///< RGBA with 32 bits each.
		A32R32G32B32, ///< ARGB with 32 bits each.

		// Special formats.
		B10G11R11_UFloat, ///< BGR with 10, 11, 11 bits as unsigned floats.
		E5B9G9R9_UFloat,  ///< BGR with 9 bits each as unsigned floats with 5 bits
		YUY2,             ///< YUY2 format.

		// Compressed formats.
		BC1_RGB,          ///< S3TC BC1 format (DXT1) with RGB.
		BC1_RGBA,         ///< S3TC BC1 format (DXT1) with RGBA with 1 bit alpha.
		BC2,              ///< S3TC BC2 format (DXT2/3).
		BC3,              ///< S3TC BC3 format (DXT4/5).
		BC4,              ///< S3TC BC4 format.
		BC5,              ///< S3TC BC5 format.
		BC6H,             ///< S3TC BC6H format.
		BC7,              ///< S3TC BC7 format.
		ETC1,             ///< ETC1 format.
		ETC2_R8G8B8,      ///< ETC2 format with RGB with 8 bits each.
		ETC2_R8G8B8A1,    ///< ETC2 format with RGBA with 8, 8, 8, 1 bits.
		ETC2_R8G8B8A8,    ///< ETC2 format with RGBA with 8 bits each.
		EAC_R11,          ///< EAC format with R with 11 bits.
		EAC_R11G11,       ///< EAC format with RG with 11 bits each.
		ASTC_4x4,         ///< ASTC with a 4x4 block.
		ASTC_5x4,         ///< ASTC with a 5x4 block.
		ASTC_5x5,         ///< ASTC with a 5x5 block.
		ASTC_6x5,         ///< ASTC with a 6x5 block.
		ASTC_6x6,         ///< ASTC with a 6x6 block.
		ASTC_8x5,         ///< ASTC with a 8x5 block.
		ASTC_8x6,         ///< ASTC with a 8x6 block.
		ASTC_8x8,         ///< ASTC with a 8x8 block.
		ASTC_10x5,        ///< ASTC with a 10x5 block.
		ASTC_10x6,        ///< ASTC with a 10x6 block.
		ASTC_10x8,        ///< ASTC with a 10x8 block.
		ASTC_10x10,       ///< ASTC with a 10x10 block.
		ASTC_12x10,       ///< ASTC with a 12x10 block.
		ASTC_12x12,       ///< ASTC with a 12x12 block.
		PVRTC1_RGB_2BPP,  ///< PVRTC1 with RGB with 2 bits per pixel.
		PVRTC1_RGBA_2BPP, ///< PVRTC1 with RGBA with 2 bits per pixel.
		PVRTC1_RGB_4BPP,  ///< PVRTC1 with RGB with 4 bits per pixel.
		PVRTC1_RGBA_4BPP, ///< PVRTC1 with RGBA with 4 bits per pixel.
		PVRTC2_RGBA_2BPP, ///< PVRTC2 with RGBA with 2 bits per pixel.
		PVRTC2_RGBA_4BPP, ///< PVRTC2 with RGBA with 4 bits per pixel.
	};

	/**
	 * @brief The type of the data stored in the texture.
	 */
	enum class Type
	{
		UNorm,  ///< Unsigned normalized integer.
		SNorm,  ///< Signed normalized integer.
		UInt,   ///< Unsigned integer.
		Int,    ///< Signed integer.
		UFloat, ///< Unsigned float.
		Float   ///< Signed float.
	};

	/**
	 * @brief Enum for the face of a cube map.
	 */
	enum class CubeFace
	{
		PosX, ///< +X
		NegX, ///< -X
		PosY, ///< +Y
		NegY, ///< -Y
		PosZ, ///< +Z
		NegZ  ///< -Z
	};

	/**
	 * @brief Enum describing the alpha format.
	 */
	enum class Alpha
	{
		None,          ///< No alpha.
		Standard,      ///< Standard alpha.
		PreMultiplied, ///< Alpha pre-multiplied with the color.
		Encoded        ///< Alpha encodes other data, not representing alpha.
	};

	/**
	 * @brief Enum to determine how to continue with a custom mip image.
	 */
	enum class MipReplacement
	{
		Once,    ///< Resume with the previous image when going down the mip chain.
		Continue ///< Continue with the new image when going down the mip chain.
	};

	/**
	 * @brief Enum for the compression quality.
	 */
	enum class Quality
	{
		Lowest, ///< Lowest quality, but fastest results.
		Low,    ///< Low quality and moderately fast.
		Normal, ///< Tradeoff between quality and speed.
		High,   ///< High quality, but moderately slow.
		Highest ///< Highest quality, but slow.
	};

	/**
	 * @brief Enum for an output texture file type.
	 */
	enum class FileType
	{
		Auto, ///< Automatically choose the format based on the extension.
		DDS,  ///< Direct Draw Surface format.
		KTX,  ///< Kronos texture format.
		PVR   ///< PowerVR texture format.
	};

	/**
	 * @brief Enum for the result of saving a file.
	 */
	enum class SaveResult
	{
		Success,       ///< File was succesfully saved.
		Invalid,       ///< Texture or parameters were invalid.
		UnknownFormat, ///< Unknown file format.
		Unsupported,   ///< The texture is unsupported for the requested file format.
		WriteError     ///< Couldn't write the file.
	};

	/**
	 * @brief Structure containing a mask for each channel.
	 */
	struct ColorMask
	{
		/**
		 * @brief Initializes the color mask with all channels enabled.
		 */
		ColorMask() : r(true), g(true), b(true), a(true) {}

		/**
		 * @brief Initializes the color mask.
		 * @param red True if the red channel is enabled.
		 * @param green True if the green channel is enabled.
		 * @param blue True if the blue channel is enabled.
		 * @param alpha True if the alpha channel is enabled.
		 */
		ColorMask(bool red, bool green, bool blue, bool alpha = true)
			: r(red), g(green), b(blue), a(alpha) {}

		bool r; ///< True if the red channel is enabled.
		bool g; ///< True if the green channel is enabled.
		bool b; ///< True if the blue channel is enabled.
		bool a; ///< True if the alpha channel is enabled.
	};

	/**
	 * @brief Structure to index to a specific image within a texture.
	 */
	struct ImageIndex
	{
		/**
		 * @brief Initializes the image index for a cube-map texture.
		 * @param inCubeFace The cube face for the image.
		 * @param inMipLevel The mip level for the image.
		 * @param inDepth The depth within a texture array or 3D texture for the image.
		 */
		explicit ImageIndex(
			CubeFace inCubeFace, unsigned int inMipLevel = 0, unsigned int inDepth = 0)
			: cubeFace(inCubeFace), mipLevel(inMipLevel), depth(inDepth)
		{
		}

		/**
		 * @brief Initializes the image index for a non-cube-map texture.
		 * @param inMipLevel The mip level for the image.
		 * @param inDepth The depth within a texture array or 3D texture for the image.
		 */
		explicit ImageIndex(unsigned int inMipLevel = 0, unsigned int inDepth = 0)
			: cubeFace(CubeFace::PosX), mipLevel(inMipLevel), depth(inDepth)
		{
		}

		/**
		 * @brief Checks if this image index is equal to another.
		 * @param other The other image index to compare.
		 * @return Whether this is equal to other.
		 */
		bool operator==(const ImageIndex& other) const
		{
			return cubeFace == other.cubeFace && mipLevel == other.mipLevel && depth == other.depth;
		}

		/**
		 * @brief Checks if this image index is not equal to another.
		 * @param other The other image index to compare.
		 * @return Whether this is not equal to other.
		 */
		bool operator!=(const ImageIndex& other) const
		{
			return !(*this == other);
		}

		/**
		 * @brief The cube face for the image.
		 */
		CubeFace cubeFace;

		/**
		 * @brief The mip level for the image.
		 */
		unsigned int mipLevel;

		/**
		 * @brief The depth within a texture array or 3D texture for the image.
		 */
		unsigned int depth;
	};

	/**
	 * @brief Wrapper to hash an ImageIndex.
	 */
	struct ImageIndexHash
	{
		/**
		 * @brief Computes the hash for an ImageIndex.
		 * @param value The value to hash.
		 * @return The hash for the ImageIndex.
		 */
		std::size_t operator()(const ImageIndex& value) const
		{
			const std::size_t combineFactor = 0x9e3779b9;
			std::hash<unsigned int> hasher;
			size_t curHash = hasher(static_cast<unsigned int>(value.cubeFace));
			curHash ^= hasher(value.mipLevel) + combineFactor + (curHash << 6) + (curHash >> 2);
			curHash ^= hasher(value.depth) + combineFactor + (curHash << 6) + (curHash >> 2);
			return curHash;
		}
	};

	/**
	 * @brief Structure for a custom image to inject when generating mipmaps.
	 */
	struct CUTTLEFISH_EXPORT CustomMipImage
	{
		/**
		 * @brief Constructs the mip image.
		 *
		 * Use this constructor if you wish to use an image externally stored.
		 *
		 * @param inImage The image to replace with.
		 * @param inReplacement How to replace the image further down the mip chain.
		 */
		CustomMipImage(const Image& inImage, MipReplacement inReplacement)
			: image(&inImage), replacement(inReplacement)
		{
		}

		/**
		 * @brief Constructs the mip image, transferring the image to this.
		 *
		 * Use this constructor if you don't otherwise need to store the image for other purposes.
		 *
		 * @param inImage The image to replace with.
		 * @param inReplacement How to replace the image further down the mip chain.
		 */
		CustomMipImage(Image&& inImage, MipReplacement inReplacement)
			: image(&imageStorage), replacement(inReplacement), imageStorage(std::move(inImage))
		{
		}

		/**
		 * @brief Copy constructor.
		 * @param other The other value to copy.
		 */
		CustomMipImage(const CustomMipImage& other);

		/**
		 * @brief Move constructor.
		 * @param other The other value to copy.
		 */
		CustomMipImage(CustomMipImage&& other);

		/**
		 * @brief Copy assignment operator.
		 * @param other The other value to copy.
		 * @return A reference to this.
		 */
		CustomMipImage& operator=(const CustomMipImage& other);

		/**
		 * @brief Move assignment operator.
		 * @param other The other value to move.
		 * @return A reference to this.
		 */
		CustomMipImage& operator=(CustomMipImage&& other);

		/**
		 * @brief A pointer to the image to replace with.
		 */
		const Image* image;

		/**
		 * @brief How to replace the image further down the mip chain.
		 */
		MipReplacement replacement;

		/**
		 * @brief Storage when an image isn't otherwise shared.
		 */
		Image imageStorage;
	};

	/**
	 * @brief Mapping from an index of a specific mip image to a custom image to replace during mip
	 * generation.
	 */
	using CustomMipImages = std::unordered_map<ImageIndex, CustomMipImage, ImageIndexHash>;

	/**
	 * @brief Constant for all mip livels.
	 */
	static const unsigned int allMipLevels = (unsigned int)-1;

	/**
	 * @brief Constant for all available cores.
	 */
	static const unsigned int allCores = (unsigned int)-1;

	/**
	 * @brief Returns whether or not a format is valid.
	 *
	 * This can be used to check if a type is compatible with a format.
	 *
	 * @remark Compressed types that have been disabled through compile flags will always return
	 *     false.
	 * @param format The base format for the texture.
	 * @param type The type for the texture data.
	 * @return True if the format and type combination is valid.
	 */
	static bool isFormatValid(Format format, Type type);

	/**
	 * @brief Returns whether or not a format is valid for a file type.
	 * @param format The base format for the texture.
	 * @param type The type for the texture data.
	 * @param fileType The file type that will be saved as.
	 * @return True if the format and type combination is valid.
	 */
	static bool isFormatValid(Format format, Type type, FileType fileType);

	/**
	 * @brief Returns whether or not a format supports native sRGB.
	 *
	 * This allows the texture to be stored in sRGB format and converted to linear during texture
	 * lookup.
	 *
	 * @param format The base format for the texture.
	 * @param type The channel type
	 * @return True if the format supports native sRGB.
	 */
	static bool hasNativeSRGB(Format format, Type type);

	/**
	 * @brief Returns whether or not the format has alpha.
	 * @param format The format to check.
	 * @return True if the format has alpha.
	 */
	static bool hasAlpha(Format format);

	/**
	 * @brief Gets the maximum number of mipmap levels.
	 * @param dimension The dimension of the texture.
	 * @param width The width of the texture.
	 * @param height The height of the texture.
	 * @param depth The depth of the texture. This will be ignored if the dimension isn't set to
	 *     Dim3D.
	 * @return The maximum number of mipmap levels.
	 */
	static unsigned int maxMipmapLevels(Dimension dimension, unsigned int width,
		unsigned int height, unsigned int depth = 0);

	/**
	 * @brief Gets the block width for a format.
	 * @param format The texture format.
	 * @return The block width.
	 */
	static unsigned int blockWidth(Format format);

	/**
	 * @brief Gets the block height for a format.
	 * @param format The texture format.
	 * @return The block height.
	 */
	static unsigned int blockHeight(Format format);

	/**
	 * @brief Gets the size of a format.
	 * @param format The texture format.
	 * @return The size.
	 */
	static unsigned int blockSize(Format format);

	/**
	 * @brief Gets the minimum width for a format.
	 * @param format The texture format.
	 * @return The minimum width.
	 */
	static unsigned int minWidth(Format format);

	/**
	 * @brief Gets the minimum height for a format.
	 * @param format The texture format.
	 * @return The minimum height.
	 */
	static unsigned int minHeight(Format format);

	/**
	 * @brief Gets the file type for a file name.
	 * @param fileName The file name to get the type for.
	 * @return The file type. If the type couldn't be found, Auto is returned.
	 */
	static FileType fileType(const char* fileName);

	/**
	 * @brief Adjusts the value range for an image.
	 *
	 * This will adjust values based on reasonable expectations from the original image format to
	 * the texture type. The current situations considered are:
	 * - Input images with a UNorm value range will be converted to [-1, 1] range when used with
	 *   SNorm type. This will convert to a float type if not already one.
	 * - Input images with a UNorm value range will be converted to the appropriate integer range.
	 *
	 * @param[inout] image The image to adjust the values for.
	 * @param type The target texture type the image will be converted to.
	 * @param origImageFormat The original image format before any conversions for internal
	 *     processing. If set to Invalid, the image's current format will be used.
	 */
	static void adjustImageValueRange(Image& image, Type type,
		Image::Format origImageFormat = Image::Format::Invalid);

	Texture();

	/**
	 * @brief Initializes a texture.
	 * @param dimension The dimension of the texture.
	 * @param width The width of the texture.
	 * @param height The height of the txture.
	 * @param depth If the dimension is Dim3D, the depth of the texture. Otherwise, it is th enumber
	 *     of array levels. If 0, the texture is not a texture array.
	 * @param colorSpace The color space of the texture.
	 * @param mipLevels The number of mipmap levels.
	 */
	Texture(Dimension dimension, unsigned int width, unsigned int height, unsigned int depth = 0,
		unsigned int mipLevels = 1, ColorSpace colorSpace = ColorSpace::Linear);

	~Texture();

	/// @cond
	Texture(const Texture& other);
	Texture(Texture&& other) noexcept;

	Texture& operator=(const Texture& other);
	Texture& operator=(Texture&& other) noexcept;
	/// @endcond

	/**
	 * @brief Returns whether or not an texture is valid.
	 * @return True if valid.
	 */
	bool isValid() const;

	/** @copydoc isValid() */
	explicit operator bool() const;

	/**
	 * @brief Initializes a texture.
	 * @param dimension The dimension of the texture.
	 * @param width The width of the texture.
	 * @param height The height of the txture.
	 * @param depth If the dimension is Dim3D, the depth of the texture. Otherwise, it is th enumber
	 *     of array levels. If 0, the texture is not a texture array.
	 * @param mipLevels The number of mipmap levels.
	 * @param colorSpace The color space of the texture.
	 * @return False if the dimensions are invalid.
	 */
	bool initialize(Dimension dimension, unsigned int width, unsigned int height,
		unsigned int depth = 0, unsigned int mipLevels = 1,
		ColorSpace colorSpace = ColorSpace::Linear);

	/**
	 * @brief Resets the texture to an unitialized state.
	 */
	void reset();

	/**
	 * @brief Gets the dimension of the texture.
	 * @return The dimension.
	 */
	Dimension dimension() const;

	/**
	 * @brief Gets the color space of the texture.
	 * @return The color space.
	 */
	ColorSpace colorSpace() const;

	/**
	 * @brief Returns whether or not this is a texture array.
	 * @return True if a texture array.
	 */
	bool isArray() const;

	/**
	 * @brief Gets the width of the texture.
	 * @param mipLevel The mip level to get the width at.
	 * @return The width of the texture, or 0 if mipLevel is outside of the mipmap range.
	 */
	unsigned int width(unsigned int mipLevel = 0) const;

	/**
	 * @brief Gets the height of the texture.
	 * @param mipLevel The mip level to get the height at.
	 * @return The height of the texture, or 0 if mipLevel is outside of the mipmap range.
	 */
	unsigned int height(unsigned int mipLevel = 0) const;

	/**
	 * @brief Gets the depth of the texture.
	 * @param mipLevel The mip level to get the depth at.
	 * @return The depth of the texture, or 0 if mipLevel is outside of the mipmap range. This will
	 *     always be at least 1 if mipLevel is in a valid range, even if this isn't a texture array.
	 */
	unsigned int depth(unsigned int mipLevel = 0) const;

	/**
	 * @brief Gets the number of mipmap levels.
	 * @return The number of mipmap levels.
	 */
	unsigned int mipLevelCount() const;

	/**
	 * @brief Gets the number of faces.
	 * @return The number of faces; 0 if uninitialized, 6 if a cube map, 1 otherwise.
	 */
	unsigned int faceCount() const;

	/**
	 * @brief Gets the image for a portion of a non-cube map texture.
	 * @param mipLevel The mipmap level.
	 * @param depth The depth level.
	 * @return The image. The image will be invalid if the parameters are invalid or the texture is
	 *     a cube map.
	 */
	const Image& getImage(unsigned int mipLevel = 0, unsigned int depth = 0) const;

	/**
	 * @brief Gets the image for a portion of a cube map texture.
	 * @param face The face to get the image for.
	 * @param mipLevel The mipmap level.
	 * @param depth The depth level.
	 * @return The image. The image will be invalid if the parameters are invalid or the texture
	 *     isn't a cube map and face isn't PosX.
	 */
	const Image& getImage(CubeFace face, unsigned int mipLevel = 0, unsigned int depth = 0) const;

	/**
	 * @brief Sets the image for a portion of a non-cube map texture.
	 * @param image The image to set. This will be converted to PixelFormat::RGBAF if not already in
	 *     that format, and it will also be converted to the texture's color space.
	 * @param mipLevel The mipmap level.
	 * @param depth The depth level.
	 * @return False if the parameters are invalid, the image is an incorrect size, or the texture
	 *     is a cube map.
	 */
	bool setImage(const Image& image, unsigned int mipLevel = 0, unsigned int depth = 0);

	/**
	 * @brief Sets the image for a portion of a non-cube map texture.
	 * @param image The image to set.
	 * @param mipLevel The mipmap level.
	 * @param depth The depth level.
	 * @return False if the parameters are invalid, the image is an incorrect size, or the texture
	 *     is a cube map.
	 */
	bool setImage(Image&& image, unsigned int mipLevel = 0, unsigned int depth = 0);

	/**
	 * @brief Sets the image for a portion of a cube map texture.
	 * @param image The image to set. This will be converted to PixelFormat::RGBAF if not already in
	 *     that format, and it will also be converted to the texture's color space.
	 * @param face The face to set the image for.
	 * @param mipLevel The mipmap level.
	 * @param depth The depth level.
	 * @return False if the parameters are invalid, the image is an incorrect size, or the texture
	 *     isn't a cube map and face isn't PosX.
	 */
	bool setImage(
		const Image& image, CubeFace face, unsigned int mipLevel = 0, unsigned int depth = 0);

	/**
	 * @brief Sets the image for a portion of a cube map texture.
	 * @param image The image to set. This will be converted to PixelFormat::RGBAF if not already in
	 *     that format, and it will also be converted to the texture's color space.
	 * @param face The face to set the image for.
	 * @param mipLevel The mipmap level.
	 * @param depth The depth level.
	 * @return False if the parameters are invalid, the image is an incorrect size or color space,
	 *     or the texture isn't a cube map and face isn't PosX.
	 */
	bool setImage(Image&& image, CubeFace face, unsigned int mipLevel = 0, unsigned int depth = 0);

	/**
	 * @brief Generates mipmaps for the texture.
	 *
	 * All of the images fro the first mip level must be set beforehand. All other existing levels
	 * will be ignored.
	 *
	 * To customize images used for specific mip levels, a mapping from image index to the custom
	 * image may be provided via customMipImages. Any images will be resized to the appropriate
	 * dimensions, and may be set to either use for that one mip image or to continue using for
	 * following images in the mip chain.
	 *
	 * @remark When using custom mip images for 3D textures, when providing custom image for a given
	 * mip level all depth images must be provided, and they must all use the same replacement value.
	 *
	 * @param filter The filter to use for resizing.
	 * @param mipLevels The number of mipmap levels to generate.
	 * @param customMipImages Custom images to use for individual mip levels.
	 * @return False if the texture isn't valid.
	 */
	bool generateMipmaps(Image::ResizeFilter filter = Image::ResizeFilter::CatmullRom,
		unsigned int mipLevels = allMipLevels, const CustomMipImages& customMipImages = {});

	/**
	 * @brief Returns whether or not all images are present for each mip level, depth level, and
	 *     face.
	 * @return True if all images are present.
	 */
	bool imagesComplete() const;

	/**
	 * @brief Converts the input images into the final texture.
	 *
	 * This will destroy the images as the texture surfaces are completed. This helps conserve
	 * memory when dealing with large textures.
	 *
	 * @param format The texture format to use.
	 * @param type The type of the data within the texture.
	 * @param quality The quality of compression.
	 * @param alphaType The type of the alpha.
	 * @param colorMask The color mask for the channels that are used. This may be used to avoid
	 *     those channels from impacting block compression.
	 * @param threads The number of threads to use during conversion.
	 * @return False if the images aren't complete, the format and type combination is invalid, the
	 *     color space cannot be used with the format, or the size is invalid for the type.
	 */
	bool convert(Format format, Type type, Quality quality = Quality::Normal,
		Alpha alphaType = Alpha::Standard, ColorMask colorMask = ColorMask(),
		unsigned int threads = allCores);

	/**
	 * @brief Returns whether or not the images have been converted into a texture.
	 * @return True if converted.
	 */
	bool converted() const;

	/**
	 * @brief Gets the format of the texture.
	 * @return The format.
	 */
	Format format() const;

	/**
	 * @brief Gets the type of the texture data.
	 * @return The type.
	 */
	Type type() const;

	/**
	 * @brief Gets the type of the alpha.
	 * @return The alpha type.
	 */
	Alpha alphaType() const;

	/**
	 * @brief Gets the color mask.
	 * @return The color mask.
	 */
	ColorMask colorMask() const;

	/**
	 * @brief Gets the data size for a portion of a non-cube map texture.
	 * @param mipLevel The mipmap level.
	 * @param depth The depth level.
	 * @return The data size, or 0 if the parameters are invalid or the texture is a cube map.
	 */
	std::size_t dataSize(unsigned int mipLevel = 0, unsigned int depth = 0) const;

	/**
	 * @brief Gets the data size for a portion of a cube map texture.
	 * @param face The face to get the image for.
	 * @param mipLevel The mipmap level.
	 * @param depth The depth level.
	 * @return The data size, or 0 if the parameters are invalid or the texture isn't a cube map and
	 *     face isn't PosX.
	 */
	std::size_t dataSize(CubeFace face, unsigned int mipLevel = 0, unsigned int depth = 0) const;

	/**
	 * @brief Gets the data for a portion of a non-cube map texture.
	 * @param mipLevel The mipmap level.
	 * @param depth The depth level.
	 * @return The data, or null if the parameters are invalid or the texture is a cube map.
	 */
	const void* data(unsigned int mipLevel = 0, unsigned int depth = 0) const;

	/**
	 * @brief Gets the size for a portion of a cube map texture.
	 * @param face The face to get the image for.
	 * @param mipLevel The mipmap level.
	 * @param depth The depth level.
	 * @return The data, or null if the parameters are invalid or the texture is a cube map.
	 */
	const void* data(CubeFace face, unsigned int mipLevel = 0, unsigned int depth = 0) const;

	/**
	 * @brief Saves a texture to a file.
	 * @param fileName The name of the file to save to.
	 * @param fileType The type of the file to save.
	 * @return False if the texture hasn't been converted or the file couldn't be saved.
	 */
	SaveResult save(const char* fileName, FileType fileType = FileType::Auto);

	/**
     * @brief Saves a texture to a stream.
     * @param stream The stream to write data to. This must be opened in binary mode!
     * @param fileType The type of the file to save.
     * @return False if the texture hasn't been converted or the data couldn't be written.
     */
    SaveResult save(std::ostream& stream, FileType fileType);

    /**
     * @brief Saves a texture to a byte vector.
     * @param[out] outData The byte vector to write data to. The contents will be overwritten.
     * @param fileType The type of the file to save.
     * @return False if the texture hasn't been converted or the data couldn't be written.
     */
    SaveResult save(std::vector<std::uint8_t>& outData, FileType fileType);

private:
	struct Impl;
	std::unique_ptr<Impl> m_impl;
};

} // cuttlefish
