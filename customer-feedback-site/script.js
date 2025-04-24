document.getElementById('submitBtn').addEventListener('click', function () {
    const name = document.getElementById('name').value;
    const email = document.getElementById('email').value;
    const content = document.getElementById('content').value;
  
    if (!name || !email || !content) {
      alert('Vui lòng điền đầy đủ thông tin');
      return;
    }
  
    const query = `
        mutation CreateFeedback($name: String!, $email: String!, $content: String!) {
        createCustomerFeedback(input: { 
            customerFeedback: { 
            name: $name, 
            email: $email, 
            content: $content 
            } 
        }) {
            customerFeedback {
            id
            submittedAt
            status
            }
        }
        }`;
  
    fetch('http://localhost:5001/graphql', { 
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        query: query,
        variables: { name, email, content },
      }),
    })
      .then((response) => {
        if (!response.ok) {
          throw new Error('GraphQL network error');
        }
        return response.json();
      })
      .then((data) => {
        if (data.errors) {
          throw new Error(data.errors.map(e => e.message).join(', '));
        }
        document.getElementById('feedbackForm').style.display = 'none';
        document.getElementById('successMessage').style.display = 'block';
      })
      .catch((error) => {
        document.getElementById('errorMessage').style.display = 'block';
        console.error('Error submitting feedback:', error);
      });
  });
  