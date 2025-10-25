float3 CalcPosition(float4 qps)
{
    if((qps[0]==0&&qps[1]==0&&qps[2]==0&&qps[3]==0)||(qps[0]==1&&qps[1]==1&&qps[2]==1&&qps[3]==1)) return float3(0,1000,0);
    float3 position = ((pow(length(_radius*qps),2)-2*pow(_radius*qps[0],2))*float3(1,1,1)-2*float3(pow(_radius*qps[1],2),pow(_radius*qps[2],2),pow(_radius*qps[3],2)))/(8*_unit);
    return position;
}

void ParametersCompression()
{
	_All[0]=_SR;
	_All[1]=_SL;
	_All[2]=_OR;
	_All[3]=_OL;
	_All[4]=_SE1;
	_All[5]=_SE2;
	_All[6]=_SE3;
	_All[7]=_OE1;
	_All[8]=_OE2;
	_All[9]=_OE3;
	_All[10]=_T;
    
    for(int i=0;i<4;i++){_ContactPosition[i]=CalcPosition(_All[i]);}
    if(_excollider==1){for(int i=4;i<10;i++){_ContactPosition[i]=CalcPosition(_All[i]);}}
    _ContactPosition[10]=CalcPosition(_All[10]);
}

float CalcContactStrength(float3 vertPos, float3 contactPos, float3 normal)
{
    float contactStrength = saturate(1-length(vertPos-contactPos-normalize(normal)*_depth)/(_effect*0.08))*lerp(0.666,1,saturate(dot(normalize(normal),normalize(vertPos-contactPos))));
    return contactStrength;
}

float3 CalcCylinderPosition(float3 vertPos, float3 contactPos1, float3 contactPos2)
{
    float3 position = contactPos1+dot(normalize(contactPos2-contactPos1),(vertPos-contactPos1))*normalize(contactPos2-contactPos1);
    return position;
}

float CalcCapSuleContactStrength(float3 vertPos, float3 contactPos1, float3 contactPos2, float3 normal)
{
    if(contactPos1.y==1000||contactPos2.y==1000) return 0;
    float3 contactPos = float3(0,0,0);
    if(dot(normalize(contactPos2-contactPos1),(vertPos-contactPos1))<=0) contactPos = contactPos1;
    else if(dot(normalize(contactPos1-contactPos2),(vertPos-contactPos2))<=0)  contactPos = contactPos2;
    else contactPos = CalcCylinderPosition(vertPos,contactPos1,contactPos2);
    return CalcContactStrength(vertPos,contactPos,normal);
}

// float CalcCapSuleContactStrength(float3 vertPos, float3 contactPos1, float3 contactPos2, float3 normal)
// {
//     if(contactPos1.y==1000||contactPos2.y==1000) return 0;
//     float contactStrength = 0;
//     int divisionsNumber = 100;
//     for(int i=1;i<divisionsNumber;i++)
//     {
//         contactStrength = max(contactStrength,CalcContactStrength(vertPos,lerp(contactPos1,contactPos2,(float)i/(float)divisionsNumber),normal));
//     }
//     return contactStrength;
// }

float MaxContactStrength(float3 position, float3 normal)
{
    float contactStrength = 0;
    for(int i=0;i<4;i++){contactStrength = max(contactStrength,CalcContactStrength(position,_ContactPosition[i],normal));}
    if(_excollider==1)
    {
        for(int i=4;i<6;i++){contactStrength = max(contactStrength,CalcCapSuleContactStrength(position,_ContactPosition[i],_ContactPosition[i+1],normal));}
        for(int i=7;i<9;i++){contactStrength = max(contactStrength,CalcCapSuleContactStrength(position,_ContactPosition[i],_ContactPosition[i+1],normal));}
    }
    contactStrength = max(contactStrength,CalcContactStrength(position,_ContactPosition[10],normal));
    return contactStrength;
}

float3 ContactStrengthToOffset(float contactStrength, float3 normal, float4 uv)
{
    float3 offset = -normalize(normal)*tex2Dlod(_Mask,uv)*_strength*0.024*(1-exp(-2.2*_func1*contactStrength))/(1+exp(-10*_func2*(contactStrength-0.4*_func3)))*(1+_multi*4);
    return offset;
}

float3 CalcOffset(float3 position, float3 normal, float4 uv)
{
    return ContactStrengthToOffset(MaxContactStrength(position,normal),normal,uv);
}

float3 CalcNewNormal(float3 position, float3 offset, float3 normal, float3 tangent, float4 uv)
{
    // if(length(offset)==0) return normal;

    float3 tan2 = normalize(tangent)*_nfunc;
    if(abs(dot(tangent,normal))>0.001f) tan2 = normalize(tangent-dot(tangent,normal)*normal)*_nfunc;
    float3 bi2 = normalize(cross(normal,tan2))*_nfunc;
    float3 positionT = tan2+CalcOffset(position + tan2,normal,uv)-offset;
    float3 positionB = bi2+CalcOffset(position + bi2,normal,uv)-offset;
    float3 newnormal = normalize(cross(positionT,positionB));
    return newnormal;
}