export interface RequestLogin {
  username: string;
  password: string;
}

export interface ResponseLogin {
  token: string;
  username: string;
  rol: string;
}
