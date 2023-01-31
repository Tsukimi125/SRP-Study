#ifndef KAISER_SURFACE_INCLUDED
#define KAISER_SURFACE_INCLUDED

struct Surface
{
    float3 position;
    float3 color;
    float3 normal;
    float3 viewDirection;
    float alpha;
    float metallic;
    float smoothness;
};

#endif