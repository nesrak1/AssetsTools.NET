#include "TextureEncoder.h"
#include "TextureFormat.h"
#include "cuttlefish/Image.h"
#include "cuttlefish/Texture.h"
#include <fstream>
#include <cstring>

using namespace std;

#if defined(_MSC_VER)
#define EXPORT extern "C" __declspec(dllexport)
#elif defined(__GNUC__)
#define EXPORT extern "C" __attribute__((visibility("default")))
#endif

struct TextureDataBuffer {
    int width;
    int height;
    int size;
    void* data;
};

cuttlefish::Texture::Format u2cfFormat(TextureFormat uf)
{
    switch (uf)
    {
        case TextureFormat::Alpha8: return cuttlefish::Texture::Format::A8; // <not supported>
        case TextureFormat::ARGB4444: return cuttlefish::Texture::Format::A4R4G4B4;
        case TextureFormat::RGB24: return cuttlefish::Texture::Format::R8G8B8;
        case TextureFormat::RGBA32: return cuttlefish::Texture::Format::R8G8B8A8;
        case TextureFormat::ARGB32: return cuttlefish::Texture::Format::A8R8G8B8; // <not supported>
        case TextureFormat::ARGBFloat: return cuttlefish::Texture::Format::A32R32G32B32; // <not supported>
        case TextureFormat::RGB565: return cuttlefish::Texture::Format::R5G6B5;
        case TextureFormat::BGR24: return cuttlefish::Texture::Format::B8G8R8;
        case TextureFormat::R16: return cuttlefish::Texture::Format::R16;
        case TextureFormat::DXT1: return cuttlefish::Texture::Format::BC1_RGB;
        case TextureFormat::DXT3: return cuttlefish::Texture::Format::BC2;
        case TextureFormat::DXT5: return cuttlefish::Texture::Format::BC3;
        case TextureFormat::RGBA4444: return cuttlefish::Texture::Format::R4G4B4A4;
        case TextureFormat::BGRA32: return cuttlefish::Texture::Format::B8G8R8A8;
        case TextureFormat::RHalf: return cuttlefish::Texture::Format::R16;
        case TextureFormat::RGHalf: return cuttlefish::Texture::Format::R16G16;
        case TextureFormat::RGBAHalf: return cuttlefish::Texture::Format::R16G16B16A16;
        case TextureFormat::RFloat: return cuttlefish::Texture::Format::R32;
        case TextureFormat::RGFloat: return cuttlefish::Texture::Format::R32G32;
        case TextureFormat::RGBAFloat: return cuttlefish::Texture::Format::R32G32B32A32;
        case TextureFormat::YUY2: return cuttlefish::Texture::Format::YUY2; // <not supported>
        case TextureFormat::RGB9e5Float: return cuttlefish::Texture::Format::E5B9G9R9_UFloat;
        case TextureFormat::RGBFloat: return cuttlefish::Texture::Format::R32G32B32;
        case TextureFormat::BC6H: return cuttlefish::Texture::Format::BC6H;
        case TextureFormat::BC7: return cuttlefish::Texture::Format::BC7;
        case TextureFormat::BC4: return cuttlefish::Texture::Format::BC4;
        case TextureFormat::BC5: return cuttlefish::Texture::Format::BC5;
        case TextureFormat::DXT1Crunched: return cuttlefish::Texture::Format::BC1_RGB; // special case
        case TextureFormat::DXT5Crunched: return cuttlefish::Texture::Format::BC3; // special case
        case TextureFormat::PVRTC_RGB2: return cuttlefish::Texture::Format::PVRTC1_RGB_2BPP;
        case TextureFormat::PVRTC_RGBA2: return cuttlefish::Texture::Format::PVRTC1_RGBA_2BPP;
        case TextureFormat::PVRTC_RGB4: return cuttlefish::Texture::Format::PVRTC1_RGB_4BPP;
        case TextureFormat::PVRTC_RGBA4: return cuttlefish::Texture::Format::PVRTC1_RGBA_4BPP;
        case TextureFormat::ETC_RGB4: return cuttlefish::Texture::Format::ETC1;
        // ATC not supported
        case TextureFormat::BGRA32Old: return cuttlefish::Texture::Format::B8G8R8A8;
        case TextureFormat::EAC_R: return cuttlefish::Texture::Format::EAC_R11;
        case TextureFormat::EAC_R_SIGNED: return cuttlefish::Texture::Format::EAC_R11;
        case TextureFormat::EAC_RG: return cuttlefish::Texture::Format::EAC_R11G11;
        case TextureFormat::EAC_RG_SIGNED: return cuttlefish::Texture::Format::EAC_R11G11;
        case TextureFormat::ETC2_RGB4: return cuttlefish::Texture::Format::ETC2_R8G8B8; // ???
        case TextureFormat::ETC2_RGBA1: return cuttlefish::Texture::Format::ETC2_R8G8B8A1;
        case TextureFormat::ETC2_RGBA8: return cuttlefish::Texture::Format::ETC2_R8G8B8A8;
        case TextureFormat::ASTC_RGB_4x4: return cuttlefish::Texture::Format::ASTC_4x4;
        case TextureFormat::ASTC_RGB_5x5: return cuttlefish::Texture::Format::ASTC_5x5;
        case TextureFormat::ASTC_RGB_6x6: return cuttlefish::Texture::Format::ASTC_6x6;
        case TextureFormat::ASTC_RGB_8x8: return cuttlefish::Texture::Format::ASTC_8x8;
        case TextureFormat::ASTC_RGB_10x10: return cuttlefish::Texture::Format::ASTC_10x10;
        case TextureFormat::ASTC_RGB_12x12: return cuttlefish::Texture::Format::ASTC_12x12;
        case TextureFormat::ASTC_RGBA_4x4: return cuttlefish::Texture::Format::ASTC_4x4;
        case TextureFormat::ASTC_RGBA_5x5: return cuttlefish::Texture::Format::ASTC_5x5;
        case TextureFormat::ASTC_RGBA_6x6: return cuttlefish::Texture::Format::ASTC_6x6;
        case TextureFormat::ASTC_RGBA_8x8: return cuttlefish::Texture::Format::ASTC_8x8;
        case TextureFormat::ASTC_RGBA_10x10: return cuttlefish::Texture::Format::ASTC_10x10;
        case TextureFormat::ASTC_RGBA_12x12: return cuttlefish::Texture::Format::ASTC_12x12;
        case TextureFormat::ETC_RGB4_3DS: return cuttlefish::Texture::Format::ETC1;
        case TextureFormat::ETC_RGBA8_3DS: return cuttlefish::Texture::Format::ETC2_R8G8B8A8;
        case TextureFormat::RG16: return cuttlefish::Texture::Format::R8G8;
        case TextureFormat::R8: return cuttlefish::Texture::Format::R8;
        case TextureFormat::ETC_RGB4Crunched: return cuttlefish::Texture::Format::ETC1; // special case
        case TextureFormat::ETC2_RGBA8Crunched: return cuttlefish::Texture::Format::ETC2_R8G8B8A8; // special case
        case TextureFormat::ASTC_HDR_4x4: return cuttlefish::Texture::Format::ASTC_4x4;
        case TextureFormat::ASTC_HDR_5x5: return cuttlefish::Texture::Format::ASTC_5x5;
        case TextureFormat::ASTC_HDR_6x6: return cuttlefish::Texture::Format::ASTC_6x6;
        case TextureFormat::ASTC_HDR_8x8: return cuttlefish::Texture::Format::ASTC_8x8;
        case TextureFormat::ASTC_HDR_10x10: return cuttlefish::Texture::Format::ASTC_10x10;
        case TextureFormat::ASTC_HDR_12x12: return cuttlefish::Texture::Format::ASTC_12x12;
        case TextureFormat::RG32: return cuttlefish::Texture::Format::R16G16;
        case TextureFormat::RGB48: return cuttlefish::Texture::Format::R16G16B16; // <not supported>
        case TextureFormat::RGBA64: return cuttlefish::Texture::Format::R16G16B16A16; // <not supported>
        default: return cuttlefish::Texture::Format::Unknown;
    }
}

cuttlefish::Texture::Type u2cfType(TextureFormat uf)
{
    switch (uf)
    {
        case TextureFormat::ARGBFloat: return cuttlefish::Texture::Type::Float;
        case TextureFormat::RHalf: return cuttlefish::Texture::Type::Float;
        case TextureFormat::RGHalf: return cuttlefish::Texture::Type::Float;
        case TextureFormat::RGBAHalf: return cuttlefish::Texture::Type::Float;
        case TextureFormat::RFloat: return cuttlefish::Texture::Type::Float;
        case TextureFormat::RGFloat: return cuttlefish::Texture::Type::Float;
        case TextureFormat::RGBAFloat: return cuttlefish::Texture::Type::Float;
        case TextureFormat::RGBFloat: return cuttlefish::Texture::Type::Float;
        case TextureFormat::RGB9e5Float: return cuttlefish::Texture::Type::UFloat;
        case TextureFormat::BC6H: return cuttlefish::Texture::Type::UFloat;
        case TextureFormat::EAC_R_SIGNED: return cuttlefish::Texture::Type::SNorm;
        case TextureFormat::EAC_RG_SIGNED: return cuttlefish::Texture::Type::SNorm;
        default: return cuttlefish::Texture::Type::UNorm;
    }
}

cuttlefish::Texture::Quality u2cfQuality(int quality)
{
    if (quality <= 1)
    {
        return cuttlefish::Texture::Quality::Lowest;
    }
    else if (quality <= 2)
    {
        return cuttlefish::Texture::Quality::Low;
    }
    else if (quality <= 3)
    {
        return cuttlefish::Texture::Quality::Normal;
    }
    else if (quality <= 4)
    {
        return cuttlefish::Texture::Quality::High;
    }
    else
    {
        return cuttlefish::Texture::Quality::Highest;
    }
}

EXPORT int SanityCheck(int num) {
    return num + 123;
}

EXPORT TextureDataBuffer ConvertAndFreeTexture(cuttlefish::Texture* texture, TextureFormat uf, int quality = 3, int mips = 1)
{
    TextureDataBuffer buffer;
    buffer.size = 0;
    buffer.data = nullptr;

    bool convertSuccess = texture->convert(u2cfFormat(uf), u2cfType(uf), u2cfQuality(quality));
    if (!convertSuccess)
    {
        std::cout << "failure converting";
        return buffer;
    }

    if (mips > 1)
    {
        bool mipSuccess = texture->generateMipmaps(cuttlefish::Image::ResizeFilter::CatmullRom, mips);
        if (!mipSuccess)
        {
            std::cout << "failure mipping";
            return buffer;
        }
    }

    buffer.width = texture->width();
    buffer.height = texture->height();
    buffer.size = (int)texture->dataSize();
    buffer.data = malloc(buffer.size);
    if (buffer.data == nullptr)
    {
        std::cout << "failure allocating buffer";
        return buffer;
    }

    memcpy(buffer.data, texture->data(), buffer.size);

    // todo: free all mips
    texture->getImage(0).~Image();
    delete texture;

    return buffer;
}

EXPORT cuttlefish::Texture* LoadTextureFromFile(char* path)
{
    cuttlefish::Image tmpImage;
    bool success = tmpImage.load(path);
    if (!success)
    {
        return nullptr;
    }

    tmpImage.flipVertical();

    auto texture = new cuttlefish::Texture(cuttlefish::Texture::Dimension::Dim2D, tmpImage.width(), tmpImage.height());
    bool setSuccess = texture->setImage(tmpImage);
    if (!setSuccess)
    {
        return nullptr;
    }

    return texture;
}

EXPORT cuttlefish::Texture* LoadTextureFromBuffer(void* data, int size, int width, int height)
{
    cuttlefish::Image tmpImage;
    bool success = tmpImage.loadRaw(data, size, width, height);
    if (!success)
    {
        return nullptr;
    }

    tmpImage.flipVertical();

    auto texture = new cuttlefish::Texture(cuttlefish::Texture::Dimension::Dim2D, tmpImage.width(), tmpImage.height());
    bool setSuccess = texture->setImage(tmpImage);
    if (!setSuccess)
    {
        return nullptr;
    }

    return texture;
}

EXPORT void FreeTextureDataBuffer(void* data)
{
    free(data);
}