curl --form text='apple' --form user_audio_file=@apple.wav --form dialect=general_american --form user_id=1234 "https://api.speechace.co/api/scoring/text/v0.1/json?key=po%2Fc4gm%2Bp4KIrcoofC5QoiFHR2BTrgfUdkozmpzHFuP%2BEuoCI1sSoDFoYOCtaxj8N6Y%2BXxYpVqtvj1EeYqmXYSp%2BfgNfgoSr5urt6%2FPQzAQwieDDzlqZhWO2qFqYKslE&user_id=002" | python -m json.tool

