module UserLoginView

    open Giraffe.GiraffeViewEngine
    open Microsoft.AspNetCore.Http
    open UserLoginModel

    let loginView model ctx =
            [
                section [ 
                    _class "hero is-success is-fullheight" 
                ] [
                    div [ 
                        _class "hero-body" 
                    ] [
                        div [ 
                            _class "container has-text-centered" 
                        ] [
                            div [ 
                                _class "column is-4 is-offset-4" 
                            ] [
                                h3 [ 
                                    _class "title has-text-black" 
                                ] [ 
                                    str "Login" 
                                ]

                                hr [ _class "login-hr" ]

                                p [ 
                                    _class "subtitle has-text-black" 
                                ] [ 
                                    str "Please login to proceed." 
                                ]

                                div [ 
                                    _class "box" 
                                ] [
                                    figure [ 
                                        _class "avatar" 
                                    ] [
                                        img [ _src "https://placehold.it/128x128" ] 
                                    ]

                                    form [ 
                                        _action "/login" 
                                        _method "POST"
                                    ] [ 
                                        div [ 
                                            _class "field" 
                                        ] [
                                            div [ 
                                                _class "control" 
                                            ] [ 
                                                input [ 
                                                    _class "input is-large"
                                                    _type "email"
                                                    _name "email"
                                                    _value model.EMail
                                                    _placeholder "Your Email" ] 
                                            ] 
                                        ]

                                        div [ 
                                            _class "field" 
                                        ] [
                                            div [ 
                                                _class "control" 
                                            ] [
                                                input [
                                                    _class "input is-large"
                                                    _type "password"
                                                    _name "password"
                                                    _value model.Password
                                                    _placeholder "Your Password" 
                                                ] 
                                            ] 
                                        ]

                                        div [ 
                                            _class "field" 
                                        ] [ 
                                            label [ 
                                                _class "checkbox" 
                                            ] [ 
                                                input [ _type "checkbox" ]
                                                str "Remember me" 
                                            ] 
                                        ]

                                        button [ 
                                            _class "button is-block is-info is-large is-fullwidth" 
                                        ] [
                                            str "Login"
                                            i [ 
                                                _class "fa fa-sign-in"
                                                attr "aria-hidden" "true"
                                            ] [ ]
                                        ] 
                                    ] 
                                ]

                                p [ 
                                    _class "has-text-grey" 
                                ] [
                                    a [ 
                                        _href "../" 
                                    ] [ 
                                        str "Sign Up" 
                                    ]

                                    rawText "&nbsp;·&nbsp;"

                                    a [ 
                                        _href "../" 
                                    ] [ 
                                        str "Forgot Password" 
                                    ]

                                    rawText "&nbsp;·&nbsp;"

                                    a [ 
                                        _href "../" 
                                    ] [ 
                                        str "Need Help?" 
                                    ] 
                                ] 
                            ] 
                        ] 
                    ] 
                ]
            ]


    let loginLayout model ctx =
        App.layout (loginView model ctx) ctx