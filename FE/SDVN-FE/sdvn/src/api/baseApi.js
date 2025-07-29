import axios from 'axios';


export const baseApi = {

/**
   * Hàm định nghĩa endpoint của api
   */
apiClient: axios.create({
    baseURL: 'https://localhost:7254/api', 
    headers: {
      'Content-Type': 'application/json',
    },
    withCredentials: true, 
  }),

/**
   * Hàm base dùng chung cho các api method post
   * @param {*} url 
   * @param {*} params 
   * @param {*} handleSuccess 
   * @param {*} handleError 
   * @param {*} handleException 
   * @param {*} isAuthen 
   * @returns 
   */
async postApi(url, params, handleSuccess, handleError, handleException, isAuthen = false) {
    try {
      const config = await this.addHeaders(isAuthen);
      const response = await this.apiClient.post(url, params, config);
  
      if (response.data) {
        if (response.data.token || response.status === 200) {
          
          if (handleSuccess && typeof handleSuccess === 'function') {
            handleSuccess(response.data);
          }
        } else {
          
          //alert(response.data.message);
          if (handleError && typeof handleError === 'function') {
            handleError(response.data);
          }
        }
      }
      return response.data;
    } catch (error) {
      alert('Có lỗi trong quá trình xử lý.');
      if (handleException && typeof handleException === 'function') {
        handleException();
      }
    }
  },
  
  
    async postAuthenApi(url, params, handleSuccess, handleError, handleException) {
      return await this.postApi(url, params, handleSuccess, handleError, handleException, true);
    },

    /**
   * Hàm base dùng chung cho các api method get
   * @param {*} url 
   * @param {*} params 
   * @param {*} handleSuccess 
   * @param {*} handleError 
   * @param {*} handleException 
   * @param {*} isAuthen 
   * @returns 
   */
  async getApi(url, params, handleSuccess, handleError, handleException, isAuthen = false) {
    try {
      const config = await this.addHeaders(isAuthen);
      if (params) {
        if (typeof params == 'object') {
          let urlParam = Object.entries(params)
          .map(([key, value]) => `${key}=${value}`)
          .join('&');
          url = `${url}?${urlParam}`; 
        }
        else {
          url = `${url}/${params}`;
        }
      }
      
      let response = await this.apiClient.get(url, config);
      if (response.data) {
        if (response.data.isSuccess) {
          if (handleSuccess && typeof handleSuccess === 'function') {
            handleSuccess(response.data.data);
          }
        } else {
          //alert(response.data.message);
          if (handleError && typeof handleError === 'function') {
            handleError(response.data.data);
          }
        }
      }
      return response.data;
    } catch (error) {
      //alert('Có lỗi trong quá trình xử lý.');
      if (handleException && typeof handleException === 'function') {
        handleException();
      }
    }
  },
  
    async getAuthenApi(url, params, handleSuccess, handleError, handleException) {
      return await this.getApi(url, params, handleSuccess, handleError, handleException, true);
    },

    /**
 * Hàm base dùng cung cho các api method Put
 * @param {*} url 
 * @param {*} params 
 * @param {*} handleSuccess 
 * @param {*} handleError 
 * @param {*} handleException 
 * @param {*} isAuthen 
 * @returns 
 */

    async putApi(url, params, handleSuccess, handleError, handleException, isAuthen = false) {
        try {
          const config = await this.addHeaders(isAuthen);
          let response;
          if (params && typeof params === 'object') {
            response = await this.apiClient.put(url, params, config);
          } else {
            response = await this.apiClient.put(url, config);
          }
          if (response.data) {
            if (response.data.isSuccess) {
              if (handleSuccess && typeof handleSuccess === 'function') {
                handleSuccess(response.data.data);
              }
            } else {
             // alert(response.data.message);
              if (handleError && typeof handleError === 'function') {
                handleError(response.data.data);
              }
            }
          }
          return response.data;
        } catch (error) {
          //alert('Có lỗi trong quá trình xử lý.');
          if (handleException && typeof handleException === 'function') {
            handleException();
          }
        }
      },
      
        async putAuthenApi(url, params, handleSuccess, handleError, handleException) {
          return await this.putApi(url, params, handleSuccess, handleError, handleException, true);
        },

    /**
   * Hàm base dùng chung cho các api method delete
   * @param {*} url 
   * @param {*} params 
   * @param {*} handleSuccess 
   * @param {*} handleError 
   * @param {*} handleException 
   * @param {*} isAuthen 
   * @returns 
   */
  async deleteApi(url, params, handleSuccess, handleError, handleException, isAuthen = false) {
    try {
      const config = await this.addHeaders(isAuthen);
      if (params) {
        if (typeof params == 'object') {
          let urlParam = Object.entries(params)
          .map(([key, value]) => `${key}=${value}`)
          .join('&');
          url = `${url}?${urlParam}`; 
        }
        else {
          url = `${url}/${params}`;
        }
      }
      
      let response = await this.apiClient.delete(url, config);
      if (response.data) {
        if (response.data.isSuccess) {
          if (handleSuccess && typeof handleSuccess === 'function') {
            handleSuccess(response.data.data);
          }
        } else {
          //alert(response.data.message);
          if (handleError && typeof handleError === 'function') {
            handleError(response.data.data);
          }
        }
      }
      return response.data;
    } catch (error) {
      //alert('Có lỗi trong quá trình xử lý.');
      if (handleException && typeof handleException === 'function') {
        handleException();
      }
    }
  },
  
    async deleteAuthenApi(url, params, handleSuccess, handleError, handleException) {
      return await this.deleteApi(url, params, handleSuccess, handleError, handleException, true);
    },
  

/**
   * Hàm thêm token vào headers
   * @param {*} isAuthen 
   * @param {*} config 
   * @returns 
   */
async addHeaders(isAuthen = false, config = {}) {
    const token = this.getTokenFromCookie();
    const headers = {
      ...config.headers,
      'Content-Type': 'application/json',
    };

    if (isAuthen && token != null) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    return { ...config, headers };
  },
/**
 * Hàm lấy token từ cookie
 * @returns 
 */
getTokenFromCookie() {
    const cookies = document.cookie.split(';');
    for (let cookie of cookies) {
      const [name, value] = cookie.trim().split('=');
      if (name === 'AuthToken') {
        return value;
      }
    }
    return null;
  },

  /**
   * Hàm định nghĩa endpoint url của api dùng trong combobox
   * @param {*} config 
   */
  buildUrlRequest(config) {
    config.url = `${config.endpoint}`;
    if (config.params) {
      if (config.method.toLowerCase() === 'get') {
        let urlParam = Object.entries(config.params)
          .map(([key, value]) => `${key}=${value}`)
          .join('&&');

        config.url = urlParam
          ? `${config.endpoint}?${urlParam}`
          : `${config.endpoint}`;
      } else {
        config.body = config.params;
      }
    }
  },

}