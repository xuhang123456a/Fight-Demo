dotnet ..\..\..\Tools\Luban\Luban.ClientServer\Luban.ClientServer.dll -j cfg ^
-- ^
--define_file ..\LubanConfigs\Defines\__root__.xml ^
--input_data_dir ..\LubanConfigs\Excels ^
--output_code_dir ..\..\Assets\Scripts\Datas ^
--output_data_dir ..\..\Datas ^
--service all ^
--gen_types "code_cs_unity_json,data_json"

pause