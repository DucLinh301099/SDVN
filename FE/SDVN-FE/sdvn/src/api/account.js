import { baseApi } from './baseApi.js';
import Api from '../api/apiConst';
import axios from 'axios';

export const account = {

    /**
   * Hàm đăng nhập 
   * home page
   * @param {*} phoneNumber 
   * @param {*} password 
   * @returns 
   */
  async login(phonenumber, password_hash, successCallback, errorCallback) {
    let params = {
      PhoneNumber:phonenumber,
      Password: password_hash,
    };
    const response = await baseApi.postApi(
      Api.login.url,
      params,
      (responseData) => {
        localStorage.setItem('role', responseData.role);
        localStorage.setItem('name', responseData.name);
        if (successCallback) successCallback(responseData);
      },
      (responseData) => {
        let errorMessage = 'Tài khoản hoặc mật khẩu sai. Vui lòng thử lại.';
        if (responseData && responseData.message) {
          errorMessage = responseData.message;
        }
        if (errorCallback) errorCallback(responseData);
      }
    );
    return response;
  },

  /**
   * Hàm đăng ký tạo mới tài khoản
   * home page
   * @param {*} firstName 
   * @param {*} lastName 
   * @param {*} email 
   * @param {*} phoneNumber 
   * @param {*} password 
   * @returns 
   */
  async register(name, email, phonenumber, password_hash) {
    let params = {
      Name: name,
      Email: email,
      PhoneNumber: phonenumber,
      Password: password_hash,
    };
    const responseData = await baseApi.postApi(Api.register.url, params);
    return responseData;
  },
}