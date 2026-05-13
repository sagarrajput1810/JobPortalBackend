†c
^C:\Users\asus\Documents\JobPortal\JobPortalBackend\JobPortal.JobService\Services\JobService.cs
	namespace 	
	JobPortal
 
. 

JobService 
. 
Services '
{		 
public

 

class

 

JobService

 
:

 
IJobService

 )
{ 
private 
readonly 
JobDbContext %
_context& .
;. /
private 
readonly !
ISendEndpointProvider .!
_sendEndpointProvider/ D
;D E
private 
readonly 
ILogger  
<  !

JobService! +
>+ ,
_logger- 4
;4 5
private 
static 
readonly 
TimeSpan  (
EventSendTimeout) 9
=: ;
TimeSpan< D
.D E
FromSecondsE P
(P Q
$numQ R
)R S
;S T
public 

JobService 
( 
JobDbContext &
context' .
,. /!
ISendEndpointProvider0 E 
sendEndpointProviderF Z
,Z [
ILogger\ c
<c d

JobServiced n
>n o
loggerp v
)v w
{ 	
_context 
= 
context 
; !
_sendEndpointProvider !
=" # 
sendEndpointProvider$ 8
;8 9
_logger 
= 
logger 
; 
} 	
public 
async 
Task 
< 
IEnumerable %
<% &
JobResponseDto& 4
>4 5
>5 6
GetAllJobsAsync7 F
(F G
)G H
{ 	
var 
jobs 
= 
await 
_context %
.% &
JobPostings& 1
.1 2
Where2 7
(7 8
j8 9
=>: <
j= >
.> ?
IsActive? G
)G H
.H I
ToListAsyncI T
(T U
)U V
;V W
return 
jobs 
. 
Select 
( 
MapToResponseDto /
)/ 0
;0 1
} 	
public 
async 
Task 
< 
JobResponseDto (
?( )
>) *
GetJobByIdAsync+ :
(: ;
int; >
id? A
)A B
{ 	
var   
job   
=   
await   
_context   $
.  $ %
JobPostings  % 0
.  0 1
	FindAsync  1 :
(  : ;
id  ; =
)  = >
;  > ?
return!! 
job!! 
!=!! 
null!! 
?!!  
MapToResponseDto!!! 1
(!!1 2
job!!2 5
)!!5 6
:!!7 8
null!!9 =
;!!= >
}"" 	
public$$ 
async$$ 
Task$$ 
<$$ 
JobResponseDto$$ (
>$$( )
CreateJobAsync$$* 8
($$8 9
JobCreateDto$$9 E
jobCreateDto$$F R
,$$R S
string$$T Z
recruiterId$$[ f
)$$f g
{%% 	
var&& 
job&& 
=&& 
new&& 

JobPosting&& $
{'' 
Title(( 
=(( 
jobCreateDto(( $
.(($ %
Title((% *
,((* +
Description)) 
=)) 
jobCreateDto)) *
.))* +
Description))+ 6
,))6 7
CompanyName** 
=** 
jobCreateDto** *
.*** +
CompanyName**+ 6
,**6 7
Location++ 
=++ 
jobCreateDto++ '
.++' (
Location++( 0
,++0 1
Salary,, 
=,, 
jobCreateDto,, %
.,,% &
Salary,,& ,
,,,, -
RecruiterId-- 
=-- 
recruiterId-- )
,--) *
	CreatedAt.. 
=.. 
DateTime.. $
...$ %
UtcNow..% +
,..+ ,
IsActive// 
=// 
true// 
}00 
;00 
_context22 
.22 
JobPostings22  
.22  !
Add22! $
(22$ %
job22% (
)22( )
;22) *
await33 
_context33 
.33 
SaveChangesAsync33 +
(33+ ,
)33, -
;33- .
await55 #
TrySendSearchEventAsync55 )
(55) *
new55* -
Uri55. 1
(551 2
$str552 R
)55R S
,55S T
new55U X
JobCreatedEvent55Y h
(55h i
job66 
.66 
Id66 
,66 
job77 
.77 
Title77 
,77 
job88 
.88 
Description88 
,88  
job99 
.99 
CompanyName99 
,99  
job:: 
.:: 
Location:: 
,:: 
job;; 
.;; 
Salary;; 
,;; 
job<< 
.<< 
RecruiterId<< 
)== 
)== 
;== 
return?? 
MapToResponseDto?? #
(??# $
job??$ '
)??' (
;??( )
}@@ 	
publicBB 
asyncBB 
TaskBB 
<BB 
boolBB 
>BB 
UpdateJobAsyncBB  .
(BB. /
intBB/ 2
idBB3 5
,BB5 6
JobUpdateDtoBB7 C
jobUpdateDtoBBD P
)BBP Q
{CC 	
varDD 
jobDD 
=DD 
awaitDD 
_contextDD $
.DD$ %
JobPostingsDD% 0
.DD0 1
	FindAsyncDD1 :
(DD: ;
idDD; =
)DD= >
;DD> ?
ifEE 
(EE 
jobEE 
==EE 
nullEE 
)EE 
returnEE #
falseEE$ )
;EE) *
jobGG 
.GG 
TitleGG 
=GG 
jobUpdateDtoGG $
.GG$ %
TitleGG% *
;GG* +
jobHH 
.HH 
DescriptionHH 
=HH 
jobUpdateDtoHH *
.HH* +
DescriptionHH+ 6
;HH6 7
jobII 
.II 
CompanyNameII 
=II 
jobUpdateDtoII *
.II* +
CompanyNameII+ 6
;II6 7
jobJJ 
.JJ 
LocationJJ 
=JJ 
jobUpdateDtoJJ '
.JJ' (
LocationJJ( 0
;JJ0 1
jobKK 
.KK 
SalaryKK 
=KK 
jobUpdateDtoKK %
.KK% &
SalaryKK& ,
;KK, -
jobLL 
.LL 
IsActiveLL 
=LL 
jobUpdateDtoLL '
.LL' (
IsActiveLL( 0
;LL0 1
awaitNN 
_contextNN 
.NN 
SaveChangesAsyncNN +
(NN+ ,
)NN, -
;NN- .
awaitPP #
TrySendSearchEventAsyncPP )
(PP) *
newPP* -
UriPP. 1
(PP1 2
$strPP2 R
)PPR S
,PPS T
newPPU X
JobUpdatedEventPPY h
(PPh i
jobQQ 
.QQ 
IdQQ 
,QQ 
jobRR 
.RR 
TitleRR 
,RR 
jobSS 
.SS 
DescriptionSS 
,SS  
jobTT 
.TT 
CompanyNameTT 
,TT  
jobUU 
.UU 
LocationUU 
,UU 
jobVV 
.VV 
SalaryVV 
,VV 
jobWW 
.WW 
IsActiveWW 
)XX 
)XX 
;XX 
returnZZ 
trueZZ 
;ZZ 
}[[ 	
public]] 
async]] 
Task]] 
<]] 
bool]] 
>]] 
DeleteJobAsync]]  .
(]]. /
int]]/ 2
id]]3 5
)]]5 6
{^^ 	
var__ 
job__ 
=__ 
await__ 
_context__ $
.__$ %
JobPostings__% 0
.__0 1
	FindAsync__1 :
(__: ;
id__; =
)__= >
;__> ?
if`` 
(`` 
job`` 
==`` 
null`` 
)`` 
return`` #
false``$ )
;``) *
jobcc 
.cc 
IsActivecc 
=cc 
falsecc  
;cc  !
awaitdd 
_contextdd 
.dd 
SaveChangesAsyncdd +
(dd+ ,
)dd, -
;dd- .
awaitff #
TrySendSearchEventAsyncff )
(ff) *
newff* -
Uriff. 1
(ff1 2
$strff2 R
)ffR S
,ffS T
newffU X
JobDeletedEventffY h
(ffh i
jobffi l
.ffl m
Idffm o
)ffo p
)ffp q
;ffq r
returnhh 
truehh 
;hh 
}ii 	
publickk 
asynckk 
Taskkk 
<kk 
IEnumerablekk %
<kk% &
JobResponseDtokk& 4
>kk4 5
>kk5 6#
GetJobsByRecruiterAsynckk7 N
(kkN O
stringkkO U
recruiterIdkkV a
)kka b
{ll 	
varmm 
jobsmm 
=mm 
awaitmm 
_contextmm %
.mm% &
JobPostingsmm& 1
.nn 
Wherenn 
(nn 
jnn 
=>nn 
jnn 
.nn 
RecruiterIdnn )
==nn* ,
recruiterIdnn- 8
)nn8 9
.oo 
ToListAsyncoo 
(oo 
)oo 
;oo 
returnpp 
jobspp 
.pp 
Selectpp 
(pp 
MapToResponseDtopp /
)pp/ 0
;pp0 1
}qq 	
privatess 
JobResponseDtoss 
MapToResponseDtoss /
(ss/ 0

JobPostingss0 :
jobss; >
)ss> ?
{tt 	
returnuu 
newuu 
JobResponseDtouu %
{vv 
Idww 
=ww 
jobww 
.ww 
Idww 
,ww 
Titlexx 
=xx 
jobxx 
.xx 
Titlexx !
,xx! "
Descriptionyy 
=yy 
jobyy !
.yy! "
Descriptionyy" -
,yy- .
CompanyNamezz 
=zz 
jobzz !
.zz! "
CompanyNamezz" -
,zz- .
Location{{ 
={{ 
job{{ 
.{{ 
Location{{ '
,{{' (
Salary|| 
=|| 
job|| 
.|| 
Salary|| #
,||# $
RecruiterId}} 
=}} 
job}} !
.}}! "
RecruiterId}}" -
,}}- .
	CreatedAt~~ 
=~~ 
job~~ 
.~~  
	CreatedAt~~  )
,~~) *
IsActive 
= 
job 
. 
IsActive '
}
ÄÄ 
;
ÄÄ 
}
ÅÅ 	
private
ÉÉ 
async
ÉÉ 
Task
ÉÉ %
TrySendSearchEventAsync
ÉÉ 2
<
ÉÉ2 3
T
ÉÉ3 4
>
ÉÉ4 5
(
ÉÉ5 6
Uri
ÉÉ6 9
endpointUri
ÉÉ: E
,
ÉÉE F
T
ÉÉG H
message
ÉÉI P
)
ÉÉP Q
where
ÉÉR W
T
ÉÉX Y
:
ÉÉZ [
class
ÉÉ\ a
{
ÑÑ 	
try
ÖÖ 
{
ÜÜ 
var
áá 
searchEndpoint
áá "
=
áá# $
await
áá% *#
_sendEndpointProvider
áá+ @
.
áá@ A
GetSendEndpoint
ááA P
(
ááP Q
endpointUri
ááQ \
)
áá\ ]
.
áá] ^
	WaitAsync
áá^ g
(
áág h
EventSendTimeout
ááh x
)
ááx y
;
ááy z
await
àà 
searchEndpoint
àà $
.
àà$ %
Send
àà% )
(
àà) *
message
àà* 1
)
àà1 2
.
àà2 3
	WaitAsync
àà3 <
(
àà< =
EventSendTimeout
àà= M
)
ààM N
;
ààN O
}
ââ 
catch
ää 
(
ää 
	Exception
ää 
ex
ää 
)
ää  
{
ãã 
_logger
åå 
.
åå 
LogError
åå  
(
åå  !
ex
åå! #
,
åå# $
$str
åå% _
,
åå_ `
endpointUri
ååa l
)
åål m
;
ååm n
}
çç 
}
éé 	
}
èè 
}êê ä
_C:\Users\asus\Documents\JobPortal\JobPortalBackend\JobPortal.JobService\Services\IJobService.cs
	namespace 	
	JobPortal
 
. 

JobService 
. 
Services '
{ 
public 

	interface 
IJobService  
{ 
Task 
< 
IEnumerable 
< 
JobResponseDto '
>' (
>( )
GetAllJobsAsync* 9
(9 :
): ;
;; <
Task 
< 
JobResponseDto 
? 
> 
GetJobByIdAsync -
(- .
int. 1
id2 4
)4 5
;5 6
Task		 
<		 
JobResponseDto		 
>		 
CreateJobAsync		 +
(		+ ,
JobCreateDto		, 8
jobCreateDto		9 E
,		E F
string		G M
recruiterId		N Y
)		Y Z
;		Z [
Task

 
<

 
bool

 
>

 
UpdateJobAsync

 !
(

! "
int

" %
id

& (
,

( )
JobUpdateDto

* 6
jobUpdateDto

7 C
)

C D
;

D E
Task 
< 
bool 
> 
DeleteJobAsync !
(! "
int" %
id& (
)( )
;) *
Task 
< 
IEnumerable 
< 
JobResponseDto '
>' (
>( )#
GetJobsByRecruiterAsync* A
(A B
stringB H
recruiterIdI T
)T U
;U V
} 
} ˆT
RC:\Users\asus\Documents\JobPortal\JobPortalBackend\JobPortal.JobService\Program.cs
var		 
builder		 
=		 
WebApplication		 
.		 
CreateBuilder		 *
(		* +
args		+ /
)		/ 0
;		0 1
var

 
allowedOrigins

 
=

 
builder

 
.

 
Configuration

 *
[

* +
$str

+ ;
]

; <
;

< =
var 
jwtKey 

= 
builder 
. 
Configuration "
[" #
$str# ,
], -
?? 
throw 
new %
InvalidOperationException *
(* +
$str+ N
)N O
;O P
builder 
. 
Services 
. 
AddControllers 
(  
)  !
;! "
builder 
. 
Services 
. 
AddCors 
( 
options  
=>! #
{ 
options 
. 
	AddPolicy 
( 
$str $
,$ %
policy 
=> 
{ 	
if 
( 
string 
. 
IsNullOrWhiteSpace )
() *
allowedOrigins* 8
)8 9
||: <
allowedOrigins= K
==L N
$strO R
)R S
{ 
policy 
. 
AllowAnyOrigin %
(% &
)& '
;' (
} 
else 
{ 
policy 
. 
WithOrigins "
(" #
allowedOrigins# 1
.1 2
Split2 7
(7 8
$char8 ;
,; <
StringSplitOptions= O
.O P
RemoveEmptyEntriesP b
|c d
StringSplitOptionse w
.w x
TrimEntries	x É
)
É Ñ
)
Ñ Ö
;
Ö Ü
} 
policy   
.   
AllowAnyHeader   !
(  ! "
)  " #
.!! 
AllowAnyMethod!! 
(!!  
)!!  !
;!!! "
}"" 	
)""	 

;""
 
}## 
)## 
;## 
builder&& 
.&& 
Services&& 
.&& 
AddDbContext&& 
<&& 
JobDbContext&& *
>&&* +
(&&+ ,
options&&, 3
=>&&4 6
options'' 
.'' 
UseSqlServer'' 
('' 
builder''  
.''  !
Configuration''! .
.''. /
GetConnectionString''/ B
(''B C
$str''C V
)''V W
,''W X

sqlOptions''Y c
=>''d f
{(( 

sqlOptions)) 
.))  
EnableRetryOnFailure)) '
())' (
maxRetryCount** 
:** 
$num** 
,** 
maxRetryDelay++ 
:++ 
TimeSpan++ #
.++# $
FromSeconds++$ /
(++/ 0
$num++0 2
)++2 3
,++3 4
errorNumbersToAdd,, 
:,, 
null,, #
),,# $
;,,$ %
}-- 
)-- 
)-- 
;-- 
builder00 
.00 
Services00 
.00 
	AddScoped00 
<00 
IJobService00 &
,00& '

JobService00( 2
>002 3
(003 4
)004 5
;005 6
builder22 
.22 
Services22 
.22 
AddMassTransit22 
(22  
x22  !
=>22" $
{33 
if44 
(44 
builder44 
.44 
Environment44 
.44 
IsDevelopment44 )
(44) *
)44* +
)44+ ,
{55 
x66 	
.66	 

UsingRabbitMq66
 
(66 
(66 
context66  
,66  !
cfg66" %
)66% &
=>66' )
{77 	
var88 

rabbitHost88 
=88 
builder88 $
.88$ %
Configuration88% 2
[882 3
$str883 B
]88B C
??88D F
$str88G R
;88R S
cfg99 
.99 
Host99 
(99 

rabbitHost99 
,99  
$str99! $
,99$ %
h99& '
=>99( *
{99+ ,
h:: 
.:: 
Username:: 
(:: 
builder:: "
.::" #
Configuration::# 0
[::0 1
$str::1 @
]::@ A
??::B D
$str::E L
)::L M
;::M N
h;; 
.;; 
Password;; 
(;; 
builder;; "
.;;" #
Configuration;;# 0
[;;0 1
$str;;1 @
];;@ A
??;;B D
$str;;E L
);;L M
;;;M N
}<< 
)<< 
;<< 
cfg== 
.== 
ConfigureEndpoints== "
(==" #
context==# *
)==* +
;==+ ,
}>> 	
)>>	 

;>>
 
}?? 
else@@ 
{AA 
varBB 

connStringBB 
=BB 
builderBB  
.BB  !
ConfigurationBB! .
[BB. /
$strBB/ L
]BBL M
;BBM N
ConsoleCC 
.CC 
	WriteLineCC 
(CC 
$"CC 
$strCC f
{CCf g
!CCg h
stringCCh n
.CCn o
IsNullOrWhiteSpace	CCo Å
(
CCÅ Ç

connString
CCÇ å
)
CCå ç
}
CCç é
"
CCé è
)
CCè ê
;
CCê ë
xEE 	
.EE	 
 
UsingAzureServiceBusEE
 
(EE 
(EE  
contextEE  '
,EE' (
cfgEE) ,
)EE, -
=>EE. 0
{FF 	
cfgGG 
.GG 
HostGG 
(GG 

connStringGG 
)GG  
;GG  !
cfgHH 
.HH !
DeployPublishTopologyHH %
=HH& '
falseHH( -
;HH- .
cfgII 
.II 
ConfigureEndpointsII "
(II" #
contextII# *
)II* +
;II+ ,
}JJ 	
)JJ	 

;JJ
 
}KK 
}LL 
)LL 
;LL 
builderOO 
.OO 
ServicesOO 
.OO 
AddAuthenticationOO "
(OO" #
JwtBearerDefaultsOO# 4
.OO4 5 
AuthenticationSchemeOO5 I
)OOI J
.OOJ K
AddJwtBearerOOK W
(OOW X
optionsPP 
=>PP 
{QQ 
optionsRR 
.RR %
TokenValidationParametersRR )
=RR* +
newRR, /%
TokenValidationParametersRR0 I
{SS 	
ValidateIssuerTT 
=TT 
trueTT !
,TT! "
ValidateAudienceUU 
=UU 
trueUU #
,UU# $
ValidateLifetimeVV 
=VV 
trueVV #
,VV# $$
ValidateIssuerSigningKeyWW $
=WW% &
trueWW' +
,WW+ ,
ValidIssuerXX 
=XX 
builderXX !
.XX! "
ConfigurationXX" /
[XX/ 0
$strXX0 <
]XX< =
,XX= >
ValidAudienceYY 
=YY 
builderYY #
.YY# $
ConfigurationYY$ 1
[YY1 2
$strYY2 @
]YY@ A
,YYA B
IssuerSigningKeyZZ 
=ZZ 
newZZ " 
SymmetricSecurityKeyZZ# 7
(ZZ7 8
Encoding[[ 
.[[ 
UTF8[[ 
.[[ 
GetBytes[[ &
([[& '
jwtKey[[' -
)[[- .
)\\ 
}]] 	
;]]	 

}^^ 
)^^ 
;^^ 
builder`` 
.`` 
Services`` 
.`` 
AddAuthorization`` !
(``! "
)``" #
;``# $
buildercc 
.cc 
Servicescc 
.cc #
AddEndpointsApiExplorercc (
(cc( )
)cc) *
;cc* +
builderdd 
.dd 
Servicesdd 
.dd 
AddSwaggerGendd 
(dd 
)dd  
;dd  !
varff 
appff 
=ff 	
builderff
 
.ff 
Buildff 
(ff 
)ff 
;ff 
usinghh 
(hh 
varhh 

scopehh 
=hh 
apphh 
.hh 
Serviceshh 
.hh  
CreateScopehh  +
(hh+ ,
)hh, -
)hh- .
{ii 
varjj 
loggerjj 
=jj 
scopejj 
.jj 
ServiceProviderjj &
.jj& '
GetRequiredServicejj' 9
<jj9 :
ILoggerjj: A
<jjA B
ProgramjjB I
>jjI J
>jjJ K
(jjK L
)jjL M
;jjM N
trykk 
{ll 
varmm 
	dbContextmm 
=mm 
scopemm 
.mm 
ServiceProvidermm -
.mm- .
GetRequiredServicemm. @
<mm@ A
JobDbContextmmA M
>mmM N
(mmN O
)mmO P
;mmP Q
	dbContextnn 
.nn 
Databasenn 
.nn 
Migratenn "
(nn" #
)nn# $
;nn$ %
loggeroo 
.oo 
LogInformationoo 
(oo 
$stroo @
)oo@ A
;ooA B
}pp 
catchqq 	
(qq
 
	Exceptionqq 
exqq 
)qq 
{rr 
loggerss 
.ss 
LogErrorss 
(ss 
exss 
,ss 
$strss F
)ssF G
;ssG H
}tt 
}uu 
ifxx 
(xx 
appxx 
.xx 
Environmentxx 
.xx 
IsDevelopmentxx !
(xx! "
)xx" #
)xx# $
{yy 
appzz 
.zz 

UseSwaggerzz 
(zz 
)zz 
;zz 
app{{ 
.{{ 
UseSwaggerUI{{ 
({{ 
){{ 
;{{ 
}|| 
app~~ 
.~~ 
UseCors~~ 
(~~ 
$str~~ 
)~~ 
;~~ 
appÇÇ 
.
ÇÇ 
UseAuthentication
ÇÇ 
(
ÇÇ 
)
ÇÇ 
;
ÇÇ 
appÉÉ 
.
ÉÉ 
UseAuthorization
ÉÉ 
(
ÉÉ 
)
ÉÉ 
;
ÉÉ 
appÖÖ 
.
ÖÖ 
MapControllers
ÖÖ 
(
ÖÖ 
)
ÖÖ 
;
ÖÖ 
appáá 
.
áá 
Run
áá 
(
áá 
)
áá 	
;
áá	 
¢
\C:\Users\asus\Documents\JobPortal\JobPortalBackend\JobPortal.JobService\Models\JobPosting.cs
	namespace 	
	JobPortal
 
. 

JobService 
. 
Models %
{ 
public 

class 

JobPosting 
{ 
[ 	
Key	 
] 
public 
int 
Id 
{ 
get 
; 
set  
;  !
}" #
[		 	
Required			 
]		 
public

 
string

 
Title

 
{

 
get

 !
;

! "
set

# &
;

& '
}

( )
=

* +
string

, 2
.

2 3
Empty

3 8
;

8 9
[ 	
Required	 
] 
public 
string 
Description !
{" #
get$ '
;' (
set) ,
;, -
}. /
=0 1
string2 8
.8 9
Empty9 >
;> ?
[ 	
Required	 
] 
public 
string 
CompanyName !
{" #
get$ '
;' (
set) ,
;, -
}. /
=0 1
string2 8
.8 9
Empty9 >
;> ?
[ 	
Required	 
] 
public 
string 
Location 
{  
get! $
;$ %
set& )
;) *
}+ ,
=- .
string/ 5
.5 6
Empty6 ;
;; <
public 
decimal 
Salary 
{ 
get  #
;# $
set% (
;( )
}* +
[ 	
Required	 
] 
public 
string 
RecruiterId !
{" #
get$ '
;' (
set) ,
;, -
}. /
=0 1
string2 8
.8 9
Empty9 >
;> ?
public 
DateTime 
	CreatedAt !
{" #
get$ '
;' (
set) ,
;, -
}. /
=0 1
DateTime2 :
.: ;
UtcNow; A
;A B
public 
bool 
IsActive 
{ 
get "
;" #
set$ '
;' (
}) *
=+ ,
true- 1
;1 2
} 
} ‘
bC:\Users\asus\Documents\JobPortal\JobPortalBackend\JobPortal.JobService\Models\Events\JobEvents.cs
	namespace 	
	JobPortal
 
. 
Shared 
. 
Events !
{ 
public 

record 
JobAppliedEvent !
(! "
int" %
ApplicationId& 3
,3 4
int5 8
JobId9 >
,> ?
string@ F
CandidateEmailG U
,U V
stringW ]
CandidateName^ k
,k l
stringm s
	ResumeUrlt }
,} ~
string	 Ö
JobTitle
Ü é
,
é è
string
ê ñ
CompanyName
ó ¢
)
¢ £
;
£ §
public 

record )
ApplicationStatusUpdatedEvent /
(/ 0
int0 3
ApplicationId4 A
,A B
stringC I
CandidateEmailJ X
,X Y
stringZ `
JobTitlea i
,i j
stringk q
	NewStatusr {
){ |
;| }
public 

record 
JobCreatedEvent !
(! "
int" %
Id& (
,( )
string* 0
Title1 6
,6 7
string8 >
Description? J
,J K
stringL R
CompanyNameS ^
,^ _
string` f
Locationg o
,o p
decimalq x
Salaryy 
,	 Ä
string
Å á
RecruiterId
à ì
)
ì î
;
î ï
public 

record 
JobUpdatedEvent !
(! "
int" %
Id& (
,( )
string* 0
Title1 6
,6 7
string8 >
Description? J
,J K
stringL R
CompanyNameS ^
,^ _
string` f
Locationg o
,o p
decimalq x
Salaryy 
,	 Ä
bool
Å Ö
IsActive
Ü é
)
é è
;
è ê
public 

record 
JobDeletedEvent !
(! "
int" %
Id& (
)( )
;) *
} ø
xC:\Users\asus\Documents\JobPortal\JobPortalBackend\JobPortal.JobService\Migrations\20260504114548_updateAzureDatabase.cs
	namespace 	
	JobPortal
 
. 

JobService 
. 

Migrations )
{ 
public 

partial 
class 
updateAzureDatabase ,
:- .
	Migration/ 8
{		 
	protected 
override 
void 
Up  "
(" #
MigrationBuilder# 3
migrationBuilder4 D
)D E
{ 	
} 	
	protected 
override 
void 
Down  $
($ %
MigrationBuilder% 5
migrationBuilder6 F
)F G
{ 	
} 	
} 
} ù#
|C:\Users\asus\Documents\JobPortal\JobPortalBackend\JobPortal.JobService\Migrations\20260422061524_InitialCreateJobService.cs
	namespace 	
	JobPortal
 
. 

JobService 
. 

Migrations )
{ 
public		 

partial		 
class		 #
InitialCreateJobService		 0
:		1 2
	Migration		3 <
{

 
	protected 
override 
void 
Up  "
(" #
MigrationBuilder# 3
migrationBuilder4 D
)D E
{ 	
migrationBuilder 
. 
CreateTable (
(( )
name 
: 
$str #
,# $
columns 
: 
table 
=> !
new" %
{ 
Id 
= 
table 
. 
Column %
<% &
int& )
>) *
(* +
type+ /
:/ 0
$str1 6
,6 7
nullable8 @
:@ A
falseB G
)G H
. 

Annotation #
(# $
$str$ 8
,8 9
$str: @
)@ A
,A B
Title 
= 
table !
.! "
Column" (
<( )
string) /
>/ 0
(0 1
type1 5
:5 6
$str7 F
,F G
nullableH P
:P Q
falseR W
)W X
,X Y
Description 
=  !
table" '
.' (
Column( .
<. /
string/ 5
>5 6
(6 7
type7 ;
:; <
$str= L
,L M
nullableN V
:V W
falseX ]
)] ^
,^ _
CompanyName 
=  !
table" '
.' (
Column( .
<. /
string/ 5
>5 6
(6 7
type7 ;
:; <
$str= L
,L M
nullableN V
:V W
falseX ]
)] ^
,^ _
Location 
= 
table $
.$ %
Column% +
<+ ,
string, 2
>2 3
(3 4
type4 8
:8 9
$str: I
,I J
nullableK S
:S T
falseU Z
)Z [
,[ \
Salary 
= 
table "
." #
Column# )
<) *
decimal* 1
>1 2
(2 3
type3 7
:7 8
$str9 H
,H I
nullableJ R
:R S
falseT Y
)Y Z
,Z [
RecruiterId 
=  !
table" '
.' (
Column( .
<. /
string/ 5
>5 6
(6 7
type7 ;
:; <
$str= L
,L M
nullableN V
:V W
falseX ]
)] ^
,^ _
	CreatedAt 
= 
table  %
.% &
Column& ,
<, -
DateTime- 5
>5 6
(6 7
type7 ;
:; <
$str= H
,H I
nullableJ R
:R S
falseT Y
)Y Z
,Z [
IsActive 
= 
table $
.$ %
Column% +
<+ ,
bool, 0
>0 1
(1 2
type2 6
:6 7
$str8 =
,= >
nullable? G
:G H
falseI N
)N O
} 
, 
constraints 
: 
table "
=># %
{ 
table 
. 

PrimaryKey $
($ %
$str% 5
,5 6
x7 8
=>9 ;
x< =
.= >
Id> @
)@ A
;A B
}   
)   
;   
}!! 	
	protected$$ 
override$$ 
void$$ 
Down$$  $
($$$ %
MigrationBuilder$$% 5
migrationBuilder$$6 F
)$$F G
{%% 	
migrationBuilder&& 
.&& 
	DropTable&& &
(&&& '
name'' 
:'' 
$str'' #
)''# $
;''$ %
}(( 	
})) 
}** Õ
\C:\Users\asus\Documents\JobPortal\JobPortalBackend\JobPortal.JobService\Data\JobDbContext.cs
	namespace 	
	JobPortal
 
. 

JobService 
. 
Data #
{ 
public 

class 
JobDbContext 
: 
	DbContext  )
{ 
public 
JobDbContext 
( 
DbContextOptions ,
<, -
JobDbContext- 9
>9 :
options; B
)B C
:D E
baseF J
(J K
optionsK R
)R S
{		 	
}

 	
public 
DbSet 
< 

JobPosting 
>  
JobPostings! ,
{- .
get/ 2
;2 3
set4 7
;7 8
}9 :
	protected 
override 
void 
OnModelCreating  /
(/ 0
ModelBuilder0 <
modelBuilder= I
)I J
{ 	
base 
. 
OnModelCreating  
(  !
modelBuilder! -
)- .
;. /
modelBuilder 
. 
Entity 
<  

JobPosting  *
>* +
(+ ,
), -
. 
Property 
( 
j 
=> 
j  
.  !
Salary! '
)' (
. 
HasColumnType 
( 
$str .
). /
;/ 0
} 	
} 
} ô(
WC:\Users\asus\Documents\JobPortal\JobPortalBackend\JobPortal.JobService\DTOs\JobDtos.cs
	namespace 	
	JobPortal
 
. 

JobService 
. 
DTOs #
{ 
public 

class 
JobCreateDto 
{ 
[ 	
Required	 
] 
public 
string 
Title 
{ 
get !
;! "
set# &
;& '
}( )
=* +
string, 2
.2 3
Empty3 8
;8 9
[		 	
Required			 
]		 
public

 
string

 
Description

 !
{

" #
get

$ '
;

' (
set

) ,
;

, -
}

. /
=

0 1
string

2 8
.

8 9
Empty

9 >
;

> ?
[ 	
Required	 
] 
public 
string 
CompanyName !
{" #
get$ '
;' (
set) ,
;, -
}. /
=0 1
string2 8
.8 9
Empty9 >
;> ?
[ 	
Required	 
] 
public 
string 
Location 
{  
get! $
;$ %
set& )
;) *
}+ ,
=- .
string/ 5
.5 6
Empty6 ;
;; <
public 
decimal 
Salary 
{ 
get  #
;# $
set% (
;( )
}* +
} 
public 

class 
JobUpdateDto 
{ 
[ 	
Required	 
] 
public 
string 
Title 
{ 
get !
;! "
set# &
;& '
}( )
=* +
string, 2
.2 3
Empty3 8
;8 9
[ 	
Required	 
] 
public 
string 
Description !
{" #
get$ '
;' (
set) ,
;, -
}. /
=0 1
string2 8
.8 9
Empty9 >
;> ?
[ 	
Required	 
] 
public 
string 
CompanyName !
{" #
get$ '
;' (
set) ,
;, -
}. /
=0 1
string2 8
.8 9
Empty9 >
;> ?
[ 	
Required	 
] 
public 
string 
Location 
{  
get! $
;$ %
set& )
;) *
}+ ,
=- .
string/ 5
.5 6
Empty6 ;
;; <
public 
decimal 
Salary 
{ 
get  #
;# $
set% (
;( )
}* +
public 
bool 
IsActive 
{ 
get "
;" #
set$ '
;' (
}) *
} 
public   

class   
JobResponseDto   
{!! 
public"" 
int"" 
Id"" 
{"" 
get"" 
;"" 
set""  
;""  !
}""" #
public## 
string## 
Title## 
{## 
get## !
;##! "
set### &
;##& '
}##( )
=##* +
string##, 2
.##2 3
Empty##3 8
;##8 9
public$$ 
string$$ 
Description$$ !
{$$" #
get$$$ '
;$$' (
set$$) ,
;$$, -
}$$. /
=$$0 1
string$$2 8
.$$8 9
Empty$$9 >
;$$> ?
public%% 
string%% 
CompanyName%% !
{%%" #
get%%$ '
;%%' (
set%%) ,
;%%, -
}%%. /
=%%0 1
string%%2 8
.%%8 9
Empty%%9 >
;%%> ?
public&& 
string&& 
Location&& 
{&&  
get&&! $
;&&$ %
set&&& )
;&&) *
}&&+ ,
=&&- .
string&&/ 5
.&&5 6
Empty&&6 ;
;&&; <
public'' 
decimal'' 
Salary'' 
{'' 
get''  #
;''# $
set''% (
;''( )
}''* +
public(( 
string(( 
RecruiterId(( !
{((" #
get(($ '
;((' (
set(() ,
;((, -
}((. /
=((0 1
string((2 8
.((8 9
Empty((9 >
;((> ?
public)) 
DateTime)) 
	CreatedAt)) !
{))" #
get))$ '
;))' (
set))) ,
;)), -
})). /
public** 
bool** 
IsActive** 
{** 
get** "
;**" #
set**$ '
;**' (
}**) *
}++ 
},, …A
dC:\Users\asus\Documents\JobPortal\JobPortalBackend\JobPortal.JobService\Controllers\JobController.cs
	namespace 	
	JobPortal
 
. 

JobService 
. 
Controllers *
{ 
[		 
ApiController		 
]		 
[

 
Route

 

(


 
$str

 
)

 
]

 
public 

class 
JobController 
:  
ControllerBase! /
{ 
private 
readonly 
IJobService $
_jobService% 0
;0 1
public 
JobController 
( 
IJobService (

jobService) 3
)3 4
{ 	
_jobService 
= 

jobService $
;$ %
} 	
[ 	
HttpGet	 
] 
public 
async 
Task 
< 
ActionResult &
<& '
IEnumerable' 2
<2 3
JobResponseDto3 A
>A B
>B C
>C D

GetAllJobsE O
(O P
)P Q
{ 	
var 
jobs 
= 
await 
_jobService (
.( )
GetAllJobsAsync) 8
(8 9
)9 :
;: ;
return 
Ok 
( 
jobs 
) 
; 
} 	
[ 	
HttpGet	 
( 
$str 
) 
] 
public 
async 
Task 
< 
ActionResult &
<& '
JobResponseDto' 5
>5 6
>6 7

GetJobById8 B
(B C
intC F
idG I
)I J
{ 	
var 
job 
= 
await 
_jobService '
.' (
GetJobByIdAsync( 7
(7 8
id8 :
): ;
;; <
if 
( 
job 
== 
null 
) 
return #
NotFound$ ,
(, -
)- .
;. /
return   
Ok   
(   
job   
)   
;   
}!! 	
[## 	
	Authorize##	 
(## 
Roles## 
=## 
$str## &
)##& '
]##' (
[$$ 	
HttpPost$$	 
]$$ 
public%% 
async%% 
Task%% 
<%% 
ActionResult%% &
<%%& '
JobResponseDto%%' 5
>%%5 6
>%%6 7
	CreateJob%%8 A
(%%A B
JobCreateDto%%B N
jobCreateDto%%O [
)%%[ \
{&& 	
var'' 
recruiterId'' 
='' 
User'' "
.''" #
FindFirstValue''# 1
(''1 2

ClaimTypes''2 <
.''< =
NameIdentifier''= K
)''K L
;''L M
if(( 
((( 
string(( 
.(( 
IsNullOrEmpty(( $
((($ %
recruiterId((% 0
)((0 1
)((1 2
return((3 9
Unauthorized((: F
(((F G
)((G H
;((H I
var** 

createdJob** 
=** 
await** "
_jobService**# .
.**. /
CreateJobAsync**/ =
(**= >
jobCreateDto**> J
,**J K
recruiterId**L W
)**W X
;**X Y
return++ 
CreatedAtAction++ "
(++" #
nameof++# )
(++) *

GetJobById++* 4
)++4 5
,++5 6
new++7 :
{++; <
id++= ?
=++@ A

createdJob++B L
.++L M
Id++M O
}++P Q
,++Q R

createdJob++S ]
)++] ^
;++^ _
},, 	
[.. 	
	Authorize..	 
(.. 
Roles.. 
=.. 
$str.. &
)..& '
]..' (
[// 	
HttpPut//	 
(// 
$str// 
)// 
]// 
public00 
async00 
Task00 
<00 
IActionResult00 '
>00' (
	UpdateJob00) 2
(002 3
int003 6
id007 9
,009 :
JobUpdateDto00; G
jobUpdateDto00H T
)00T U
{11 	
var33 
job33 
=33 
await33 
_jobService33 '
.33' (
GetJobByIdAsync33( 7
(337 8
id338 :
)33: ;
;33; <
if44 
(44 
job44 
==44 
null44 
)44 
return44 #
NotFound44$ ,
(44, -
)44- .
;44. /
var66 
recruiterId66 
=66 
User66 "
.66" #
FindFirstValue66# 1
(661 2

ClaimTypes662 <
.66< =
NameIdentifier66= K
)66K L
;66L M
if77 
(77 
job77 
.77 
RecruiterId77 
!=77  "
recruiterId77# .
)77. /
return770 6
Forbid777 =
(77= >
)77> ?
;77? @
var99 
result99 
=99 
await99 
_jobService99 *
.99* +
UpdateJobAsync99+ 9
(999 :
id99: <
,99< =
jobUpdateDto99> J
)99J K
;99K L
if:: 
(:: 
!:: 
result:: 
):: 
return:: 
NotFound::  (
(::( )
)::) *
;::* +
return;; 
	NoContent;; 
(;; 
);; 
;;; 
}<< 	
[>> 	
	Authorize>>	 
(>> 
Roles>> 
=>> 
$str>> &
)>>& '
]>>' (
[?? 	

HttpDelete??	 
(?? 
$str?? 
)?? 
]?? 
public@@ 
async@@ 
Task@@ 
<@@ 
IActionResult@@ '
>@@' (
	DeleteJob@@) 2
(@@2 3
int@@3 6
id@@7 9
)@@9 :
{AA 	
varBB 
jobBB 
=BB 
awaitBB 
_jobServiceBB '
.BB' (
GetJobByIdAsyncBB( 7
(BB7 8
idBB8 :
)BB: ;
;BB; <
ifCC 
(CC 
jobCC 
==CC 
nullCC 
)CC 
returnCC #
NotFoundCC$ ,
(CC, -
)CC- .
;CC. /
varEE 
recruiterIdEE 
=EE 
UserEE "
.EE" #
FindFirstValueEE# 1
(EE1 2

ClaimTypesEE2 <
.EE< =
NameIdentifierEE= K
)EEK L
;EEL M
ifFF 
(FF 
jobFF 
.FF 
RecruiterIdFF 
!=FF  "
recruiterIdFF# .
)FF. /
returnFF0 6
ForbidFF7 =
(FF= >
)FF> ?
;FF? @
varHH 
resultHH 
=HH 
awaitHH 
_jobServiceHH *
.HH* +
DeleteJobAsyncHH+ 9
(HH9 :
idHH: <
)HH< =
;HH= >
ifII 
(II 
!II 
resultII 
)II 
returnII 
NotFoundII  (
(II( )
)II) *
;II* +
returnJJ 
	NoContentJJ 
(JJ 
)JJ 
;JJ 
}KK 	
[MM 	
HttpGetMM	 
(MM 
$strMM *
)MM* +
]MM+ ,
publicNN 
asyncNN 
TaskNN 
<NN 
ActionResultNN &
<NN& '
IEnumerableNN' 2
<NN2 3
JobResponseDtoNN3 A
>NNA B
>NNB C
>NNC D
GetJobsByRecruiterNNE W
(NNW X
stringNNX ^
recruiterIdNN_ j
)NNj k
{OO 	
varPP 
jobsPP 
=PP 
awaitPP 
_jobServicePP (
.PP( )#
GetJobsByRecruiterAsyncPP) @
(PP@ A
recruiterIdPPA L
)PPL M
;PPM N
returnQQ 
OkQQ 
(QQ 
jobsQQ 
)QQ 
;QQ 
}RR 	
}SS 
}TT 